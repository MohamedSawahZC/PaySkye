using NotificationProcessor.API.Models;
using NotificationService.API.Services;
using NotificationService.Contracts.Models;

namespace NotificationProcessor.API.Services;

public class NotificationGrpcService : INotificationGrpcService
{
    private readonly INotificationProducer _producer;
    private readonly ILogger<NotificationGrpcService> _logger;

    public NotificationGrpcService(
        INotificationProducer producer,
        ILogger<NotificationGrpcService> logger)
    {
        _producer = producer;
        _logger = logger;
    }

    public async Task<GrpcNotificationResponse> SendNotificationAsync(
        GrpcNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var contractRequest = new NotificationRequest
        {
            Title = request.Title,
            Content = request.Content,
            Type = request.Type,
            Priority = request.Priority,
            To = request.To,
            Endpoint = request.Endpoint,
            TraceId = request.TraceId
        };

        var notification = new NotificationMessage
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Request = contractRequest,
            Status = "Pending"
        };

        try
        {
            await _producer.PublishAsync(notification, cancellationToken);

            return new GrpcNotificationResponse
            {
                NotificationId = notification.Id.ToString(),
                Status = notification.Status,
                CreatedAt = notification.CreatedAt.ToString("O")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing gRPC notification request");
            throw;
        }
    }
}