using NotificationService.Contracts.Models;

namespace NotificationService.API.Services;

public interface INotificationProducer
{
    Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken = default);
}