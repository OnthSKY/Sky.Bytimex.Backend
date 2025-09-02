using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Suppliers;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Suppliers;

public class CreateSupplierRequestValidator : AbstractValidator<CreateSupplierRequest>
{
    public CreateSupplierRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage(SharedResourceKeys.InvalidEmail);
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage(SharedResourceKeys.Required)
            .Must(s => s == "ACTIVE" || s == "INACTIVE").WithMessage(SharedResourceKeys.InvalidStatus);
        RuleFor(x => x.Notes)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage(SharedResourceKeys.MaxLength);
    }
}
