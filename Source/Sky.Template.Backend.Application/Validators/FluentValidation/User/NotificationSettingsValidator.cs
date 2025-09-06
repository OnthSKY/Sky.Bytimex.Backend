using FluentValidation;
using Sky.Template.Backend.Contract.Responses.UserResponses;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.User;

public class NotificationSettingsValidator : AbstractValidator<NotificationSettingsDto>
{
    public NotificationSettingsValidator()
    {
        RuleFor(x => x.EmailNotifications).NotNull();
        RuleFor(x => x.SmsNotifications).NotNull();
        RuleFor(x => x.PushNotifications).NotNull();
        RuleFor(x => x.NewsletterOptIn).NotNull();
    }
}
