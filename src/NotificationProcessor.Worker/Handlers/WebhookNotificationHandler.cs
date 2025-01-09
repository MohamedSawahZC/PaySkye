using System.Net.Http.Json;
using NotificationService.Contracts.Models;
using System.Text.Json;

namespace NotificationProcessor.Worker.Handlers;

public class WebhookNotificationHandler : INotificationHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookNotificationHandler> _logger;

    public WebhookNotificationHandler(
        IHttpClientFactory httpClientFactory,
        ILogger<WebhookNotificationHandler> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public bool CanHandle(NotificationMessage notification)
    {
        if (notification?.Request == null)
            return false;

        return notification.Request.Type == NotificationType.WEBHOOK;
    }

    public async Task HandleAsync(NotificationMessage notification, CancellationToken cancellationToken)
    {
        if (notification?.Request?.Endpoint == null)
        {
            _logger.LogError("Cannot process webhook notification: missing endpoint URL");
            return;
        }

        _logger.LogInformation(
            "Processing WEBHOOK notification {Id} to endpoint {Endpoint}",
            notification.Id,
            notification.Request.Endpoint);

        var httpClient = _httpClientFactory.CreateClient();

        var payload = new
        {
            notification.Id,
            notification.CreatedAt,
            notification.Request.Title,
            notification.Request.Content,
            notification.Request.TraceId,
            Priority = notification.Request.Priority
        };

        try
        {
            var response = await httpClient.PostAsJsonAsync(
                notification.Request.Endpoint,
                payload,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            _logger.LogInformation(
                "WEBHOOK notification {Id} delivered successfully to {Endpoint}",
                notification.Id,
                notification.Request.Endpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to deliver WEBHOOK notification {Id} to {Endpoint}",
                notification.Id,
                notification.Request.Endpoint);
            throw;
        }
    }
}