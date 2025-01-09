namespace NotificationService.Contracts.Models;

public class NotificationMessage
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public NotificationRequest Request { get; set; } = null!;
    public string Status { get; set; } = "Pending";
}