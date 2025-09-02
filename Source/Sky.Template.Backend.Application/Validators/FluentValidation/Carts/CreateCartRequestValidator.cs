using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Carts;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Carts;

public class CreateCartRequestValidator : AbstractValidator<CreateCartRequest>
{
    public CreateCartRequestValidator()
    {
        RuleFor(x => x.BuyerId).NotEmpty();
        RuleFor(x => x.Currency).NotEmpty();
    }
}
