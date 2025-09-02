using FluentValidation;
using Sky.Template.Backend.Application.IntegrationModels.Common;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.RestIntegration;
public class RestIntegrationRequestValidator : AbstractValidator<RestIntegrationRequest>
{

    public RestIntegrationRequestValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage(SharedResourceKeys.ValidationUrlRequired)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage(SharedResourceKeys.ValidationUrlInvalid);

        RuleFor(x => x.Method)
            .NotNull().WithMessage(SharedResourceKeys.ValidationMethodRequired);

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage(SharedResourceKeys.ValidationContentTypeRequired)
            .Must(ct => ct == "application/json")
            .WithMessage(SharedResourceKeys.ValidationContentTypeUnsupported);

        RuleFor(x => x.AuthToken)
            .NotEmpty()
            .When(x => x.RequiresAuthentication)
            .WithMessage(SharedResourceKeys.ValidationAuthTokenRequiredIfAuthenticated);
    }
}
