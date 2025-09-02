using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Vendors;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Vendors;

public class CreateVendorRequestValidator : AbstractValidator<CreateVendorRequest>
{
    public CreateVendorRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage(SharedResourceKeys.InvalidEmail);
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage(SharedResourceKeys.Required)
            .Must(s => s == "ACTIVE" || s == "INACTIVE").WithMessage(SharedResourceKeys.InvalidStatus);
    }
}
