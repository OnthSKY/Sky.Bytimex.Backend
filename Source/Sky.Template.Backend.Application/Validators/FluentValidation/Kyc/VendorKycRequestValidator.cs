using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Kyc;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Kyc;

public class VendorKycRequestValidator : AbstractValidator<VendorKycRequest>
{
    public VendorKycRequestValidator()
    {
        RuleFor(x => x.LegalName)
            .NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.TaxId)
            .NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.Documents)
            .NotNull().WithMessage(SharedResourceKeys.Required)
            .Must(d => d.Count > 0).WithMessage(SharedResourceKeys.Required);
    }
}

