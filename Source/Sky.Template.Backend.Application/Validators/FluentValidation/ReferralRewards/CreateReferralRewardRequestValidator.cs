using FluentValidation;
using Sky.Template.Backend.Contract.Requests.ReferralRewards;
using Sky.Template.Backend.Core.Enums;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.ReferralRewards;

public class CreateReferralRewardRequestValidator : AbstractValidator<CreateReferralRewardRequest>
{
    public CreateReferralRewardRequestValidator()
    {
        RuleFor(x => x.ReferredUserId).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.ReferrerUserId).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.RewardType).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.RewardStatus)
            .IsInEnum().When(x => x.RewardStatus.HasValue)
            .WithMessage(SharedResourceKeys.InvalidStatus);
        RuleFor(x => x.RewardCurrency)
            .Matches("^[A-Z]{3}$").When(x => !string.IsNullOrEmpty(x.RewardCurrency))
            .WithMessage(SharedResourceKeys.InvalidCurrencyCode);
    }
}
