using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Kyc;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Kyc;

public class SubmitVendorKycRequestValidator : AbstractValidator<SubmitVendorKycRequest>
{
    public SubmitVendorKycRequestValidator()
    {
        RuleFor(x => x.VendorId)
            .NotEmpty().WithMessage(SharedResourceKeys.Required);

        RuleFor(x => x.DocumentType)
            .IsInEnum().WithMessage(SharedResourceKeys.InvalidStatus);

        RuleFor(x => x.DocumentUrl)
            .NotEmpty().WithMessage(SharedResourceKeys.Required);
    }
}

