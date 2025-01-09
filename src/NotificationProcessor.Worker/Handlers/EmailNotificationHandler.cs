using NotificationService.Contracts.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;

namespace NotificationProcessor.Worker.Handlers;

public class EmailNotificationHandler : INotificationHandler
{
    private readonly ISendGridClient _sendGridClient;
    private readonly ILogger<EmailNotificationHandler> _logger;
    private const string FROM_EMAIL = "kokekitchen7@gmail.com";

    public EmailNotificationHandler(
        ISendGridClient sendGridClient,
        ILogger<EmailNotificationHandler> logger)
    {
        _sendGridClient = sendGridClient ?? throw new ArgumentNullException(nameof(sendGridClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool CanHandle(NotificationMessage notification)
    {
        return notification?.Request?.Type == NotificationType.EMAIL;
    }

    public async Task HandleAsync(NotificationMessage notification, CancellationToken cancellationToken)
    {
        if (notification?.Request?.To == null || string.IsNullOrEmpty(notification.Request.Content))
        {
            _logger.LogError("Cannot process email notification: missing recipient email or content");
            return;
        }

        _logger.LogInformation(
            "Processing EMAIL notification {Id} to {Recipient}",
            notification.Id,
            notification.Request.To);

        var from = new EmailAddress(FROM_EMAIL, "Notification System");
        var to = new EmailAddress(notification.Request.To);
        var subject = notification.Request.Title ?? "No Subject";
        var plainTextContent = notification.Request.Content;
        var htmlContent = notification.Request.Content;

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

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
                throw new Exception($"SendGrid API returned {response.StatusCode}: {body}");
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
