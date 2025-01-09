using Confluent.Kafka;
using NotificationProcessor.Worker.Handlers;
using NotificationService.Contracts.Models;
using System.Text.Json;

namespace NotificationProcessor.Worker;

public class Worker : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IConsumer<string, string> consumer,
        IServiceProvider serviceProvider,
        ILogger<Worker> logger)
    {
        _consumer = consumer;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("notifications");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);

                    if (consumeResult?.Message == null) continue;

                    var notification = JsonSerializer.Deserialize<NotificationMessage>(
                        consumeResult.Message.Value);

                    if (notification == null)
                    {
                        _logger.LogWarning("Received null notification from message: {Value}",
                            consumeResult.Message.Value);
                        continue;
                    }

                    await ProcessNotificationAsync(notification, stoppingToken);

                    _consumer.Commit(consumeResult);

                    _logger.LogInformation(
                        "Successfully processed notification {Id}",
                        notification.Id);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                }
            }
        }
        finally
        {
            _consumer.Close();
        }
    }

    private async Task ProcessNotificationAsync(
        NotificationMessage notification,
        CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetRequiredService<IEnumerable<INotificationHandler>>();

        var handler = handlers.FirstOrDefault(h => h.CanHandle(notification));

        if (handler == null)
        {
            _logger.LogWarning(
                "No handler found for notification type {Type}",
                notification.Request.Type);
            return;
        }

        await handler.HandleAsync(notification, cancellationToken);
    }
}