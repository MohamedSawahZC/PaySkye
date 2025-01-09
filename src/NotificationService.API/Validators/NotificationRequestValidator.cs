using FluentValidation;
using NotificationService.Contracts.Models;

namespace NotificationService.API.Validators;

public class NotificationRequestValidator : AbstractValidator<NotificationRequest>
{
    public NotificationRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 10);

        When(x => x.Type == NotificationType.EMAIL, () => {
            RuleFor(x => x.To)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Valid email address is required for EMAIL notifications");
        });

        When(x => x.Type == NotificationType.WEBHOOK, () => {
            RuleFor(x => x.Endpoint)
                .NotEmpty()
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Valid endpoint URL is required for WEBHOOK notifications");

            RuleFor(x => x.TraceId)
                .NotEmpty()
                .WithMessage("TraceId is required for WEBHOOK notifications");
        });
    }
}