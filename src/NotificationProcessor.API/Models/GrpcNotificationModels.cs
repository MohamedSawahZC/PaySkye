using NotificationService.Contracts.Models;

namespace NotificationProcessor.API.Models;

public class GrpcNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public int Priority { get; set; }
    public string? To { get; set; }
    public string? Endpoint { get; set; }
    public string? TraceId { get; set; }
}

public class GrpcNotificationResponse
{
    public string NotificationId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}

public interface INotificationGrpcService
{
    Task<GrpcNotificationResponse> SendNotificationAsync(GrpcNotificationRequest request, CancellationToken cancellationToken);
}