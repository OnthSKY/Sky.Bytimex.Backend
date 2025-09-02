using FluentValidation;
using Sky.Template.Backend.Contract.Requests.CartItems;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.CartItems;

public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
{
    public UpdateCartItemRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().When(x => x.ProductId.HasValue);
        RuleFor(x => x.Quantity).GreaterThan(0).When(x => x.Quantity.HasValue);
    }
}
