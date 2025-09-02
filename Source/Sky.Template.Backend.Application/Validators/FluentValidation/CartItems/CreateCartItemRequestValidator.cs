using FluentValidation;
using Sky.Template.Backend.Contract.Requests.CartItems;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.CartItems;

public class CreateCartItemRequestValidator : AbstractValidator<CreateCartItemRequest>
{
    public CreateCartItemRequestValidator()
    {
        RuleFor(x => x.CartId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
