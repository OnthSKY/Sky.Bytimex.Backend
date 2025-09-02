using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Discounts;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Discounts;

public class CreateDiscountRequestValidator : AbstractValidator<CreateDiscountRequest>
{
    public CreateDiscountRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.DiscountType).NotEmpty();
        RuleFor(x => x.Value).GreaterThan(0);
        RuleFor(x => x.StartDate).LessThan(x => x.EndDate);
    }
}
