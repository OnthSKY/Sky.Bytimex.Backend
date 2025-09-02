using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Kyc;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Kyc;

public class KycVerificationRequestValidator : AbstractValidator<KycVerificationRequest>
{
    public KycVerificationRequestValidator()
    {
        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.Country)
            .NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.DocumentType)
            .NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.DocumentExpiryDate)
            .Must(d => d == null || d > DateTime.UtcNow).WithMessage(SharedResourceKeys.InvalidDate);
    }
}
