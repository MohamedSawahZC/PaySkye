namespace NotificationService.Contracts.Models;

public class NotificationRequest
{
    public required string Title { get; set; }
    public required string Content { get; set; }
    public NotificationType Type { get; set; }
    public int Priority { get; set; } = 1;

    public string? To { get; set; }
    public string? Endpoint { get; set; }
    public string? TraceId { get; set; }
}