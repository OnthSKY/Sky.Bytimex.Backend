using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Discounts;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Discounts;

public class UpdateDiscountRequestValidator : AbstractValidator<UpdateDiscountRequest>
{
    public UpdateDiscountRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        Include(new CreateDiscountRequestValidator());
    }
}
