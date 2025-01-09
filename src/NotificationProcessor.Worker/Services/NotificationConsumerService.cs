using Confluent.Kafka;
using NotificationProcessor.Worker.Handlers;
using NotificationService.Contracts.Models;
using System.Text.Json;

namespace NotificationProcessor.Worker.Services;

public class NotificationConsumerService : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationConsumerService> _logger;

    public NotificationConsumerService(
        IConsumer<string, string> consumer,
        IServiceProvider serviceProvider,
        ILogger<NotificationConsumerService> logger)
    {
        _consumer = consumer;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _consumer.Subscribe("notifications");
            _logger.LogInformation("Started listening for notifications on topic: notifications");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    if (consumeResult?.Message?.Value == null)
                    {
                        _logger.LogWarning("Received null message from Kafka");
                        continue;
                    }

                    _logger.LogInformation("Received message: {Value}", consumeResult.Message.Value);

                    var notification = JsonSerializer.Deserialize<NotificationMessage>(
                        consumeResult.Message.Value);

                    if (notification == null)
                    {
                        _logger.LogWarning("Failed to deserialize notification message");
                        continue;
                    }

                    await ProcessNotificationAsync(notification, stoppingToken);

                    _consumer.Commit(consumeResult);
                    _logger.LogInformation(
                        "Successfully processed and committed notification {Id}",
                        notification.Id);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming Kafka message");
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing notification message");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing notification");
                }

                await Task.Delay(100, stoppingToken); // Small delay to prevent tight loop
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in notification consumer");
            throw;
        }
        finally
        {
            try
            {
                _consumer.Close();
                _logger.LogInformation("Kafka consumer closed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing Kafka consumer");
            }
        }
    }

    private async Task ProcessNotificationAsync(
        NotificationMessage notification,
        CancellationToken cancellationToken)
    {
        if (notification == null)
        {
            _logger.LogWarning("Cannot process null notification");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetRequiredService<IEnumerable<INotificationHandler>>();

        if (!handlers.Any())
        {
            _logger.LogWarning("No notification handlers registered");
            return;
        }

        var handler = handlers.FirstOrDefault(h => h.CanHandle(notification));

        if (handler == null)
        {
            _logger.LogWarning(
                "No handler found for notification type {Type}",
                notification.Request?.Type);
            return;
        }

        try
        {
            await handler.HandleAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error handling notification {Id} with handler {HandlerType}",
                notification.Id,
                handler.GetType().Name);
            throw;
        }
    }
}