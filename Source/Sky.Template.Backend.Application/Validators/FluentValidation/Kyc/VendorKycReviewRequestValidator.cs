using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Kyc;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Kyc;

public class VendorKycReviewRequestValidator : AbstractValidator<VendorKycReviewRequest>
{
    public VendorKycReviewRequestValidator()
    {
        RuleFor(x => x.VendorId)
            .NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage(SharedResourceKeys.Required)
            .When(x => !x.Approve);
    }
}

