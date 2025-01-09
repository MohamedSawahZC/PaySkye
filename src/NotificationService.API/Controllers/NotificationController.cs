using Microsoft.AspNetCore.Mvc;
using NotificationService.API.Services;
using NotificationService.Contracts.Models;
using FluentValidation;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationProducer _producer;
    private readonly IValidator<NotificationRequest> _validator;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        INotificationProducer producer,
        IValidator<NotificationRequest> validator,
        ILogger<NotificationController> logger)
    {
        _producer = producer;
        _validator = validator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> SendNotification(
        [FromBody] NotificationRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var notification = new NotificationMessage
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Request = request,
            Status = "Pending"
        };

        try
        {
            await _producer.PublishAsync(notification, cancellationToken);
            _logger.LogInformation("Notification {Id} sent successfully", notification.Id);
            return Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification {Id}", notification.Id);
            return StatusCode(500, "Error processing notification");
        }
    }
}