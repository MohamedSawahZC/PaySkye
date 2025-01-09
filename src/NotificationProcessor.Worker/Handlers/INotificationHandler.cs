using NotificationService.Contracts.Models;

namespace NotificationProcessor.Worker.Handlers;

public interface INotificationHandler
{
    bool CanHandle(NotificationMessage notification);
    Task HandleAsync(NotificationMessage notification, CancellationToken cancellationToken);
}