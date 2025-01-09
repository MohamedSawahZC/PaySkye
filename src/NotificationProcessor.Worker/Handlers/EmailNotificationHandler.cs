using NotificationService.Contracts.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NotificationProcessor.Worker.Handlers;

public class EmailNotificationHandler : INotificationHandler
{
    private readonly ISendGridClient _sendGridClient;
    private readonly ILogger<EmailNotificationHandler> _logger;
    private const string FROM_EMAIL = "sawah810@gmail.com";

    public EmailNotificationHandler(
        ILogger<EmailNotificationHandler> logger)
    {
        _sendGridClient = new SendGridClient("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");
        _logger = logger;
    }

    public bool CanHandle(NotificationMessage notification)
    {
        if (notification?.Request == null)
            return false;

        return notification.Request.Type == NotificationType.EMAIL;
    }

    public async Task HandleAsync(NotificationMessage notification, CancellationToken cancellationToken)
    {
        if (notification?.Request?.To == null)
        {
            _logger.LogError("Cannot process email notification: missing recipient email");
            return;
        }

        _logger.LogInformation(
            "Processing EMAIL notification {Id} to {Recipient}",
            notification.Id,
            notification.Request.To);

        var msg = new SendGridMessage()
        {
            From = new EmailAddress(FROM_EMAIL, "Notification System"),
            Subject = notification.Request.Title,
            PlainTextContent = notification.Request.Content,
            HtmlContent = notification.Request.Content
        };
        msg.AddTo(new EmailAddress(notification.Request.To));

        try
        {
            var response = await _sendGridClient.SendEmailAsync(msg, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "EMAIL notification {Id} sent successfully to {Recipient}",
                    notification.Id,
                    notification.Request.To);
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Failed to send EMAIL notification {Id} to {Recipient}. Status: {StatusCode}, Body: {Body}",
                    notification.Id,
                    notification.Request.To,
                    response.StatusCode,
                    body);
                throw new Exception($"SendGrid API returned {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error sending EMAIL notification {Id} to {Recipient}",
                notification.Id,
                notification.Request.To);
            throw;
        }
    }
}