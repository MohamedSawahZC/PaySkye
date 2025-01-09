using Confluent.Kafka;
using NotificationService.API.Infrastructure;
using NotificationService.Contracts.Models;
using System.Text.Json;

namespace NotificationService.API.Services;

public class KafkaNotificationProducer : INotificationProducer
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaConfiguration _config;
    private readonly ILogger<KafkaNotificationProducer> _logger;

    public KafkaNotificationProducer(
        IProducer<string, string> producer,
        KafkaConfiguration config,
        ILogger<KafkaNotificationProducer> logger)
    {
        _producer = producer;
        _config = config;
        _logger = logger;
    }

    public async Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = JsonSerializer.Serialize(message);
            var headers = new Headers
            {
                { "priority", BitConverter.GetBytes(message.Request.Priority) }
            };

            var kafkaMessage = new Message<string, string>
            {
                Key = message.Id.ToString(),
                Value = value,
                Headers = headers
            };

            var deliveryResult = await _producer.ProduceAsync(
                _config.Topic,
                kafkaMessage,
                cancellationToken);

            _logger.LogInformation(
                "Message {MessageId} delivered to {TopicPartitionOffset}",
                message.Id,
                deliveryResult.TopicPartitionOffset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message {MessageId}", message.Id);
            throw;
        }
    }
}