using FluentValidation;
using Sky.Template.Backend.Contract.Requests.DiscountUsages;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.DiscountUsages;

public class CreateDiscountUsageRequestValidator : AbstractValidator<CreateDiscountUsageRequest>
{
    public CreateDiscountUsageRequestValidator()
    {
        RuleFor(x => x.DiscountId).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x).Must(r => r.BuyerId.HasValue || r.OrderId.HasValue)
            .WithMessage(SharedResourceKeys.EitherBuyerIdOrOrderIdMustBeProvided);
    }
}
