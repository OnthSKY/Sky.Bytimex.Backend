using FluentValidation;
using Sky.Template.Backend.Contract.Requests.ReferralRewards;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.ReferralRewards;

public class UpdateReferralRewardRequestValidator : AbstractValidator<UpdateReferralRewardRequest>
{
    public UpdateReferralRewardRequestValidator()
    {
        RuleFor(x => x.RewardStatus).IsInEnum().WithMessage(SharedResourceKeys.InvalidStatus);
        RuleFor(x => x.RewardCurrency)
            .Matches("^[A-Z]{3}$").When(x => !string.IsNullOrEmpty(x.RewardCurrency))
            .WithMessage(SharedResourceKeys.InvalidCurrencyCode);
    }
}
