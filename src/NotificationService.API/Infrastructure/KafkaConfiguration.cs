namespace NotificationService.API.Infrastructure;

public class KafkaConfiguration
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string Topic { get; set; } = "notifications";
    public string GroupId { get; set; } = "notification-service";
}