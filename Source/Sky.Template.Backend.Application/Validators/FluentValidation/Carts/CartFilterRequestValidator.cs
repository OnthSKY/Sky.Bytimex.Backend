using Sky.Template.Backend.Application.Validators.FluentValidation;
using Sky.Template.Backend.Contract.Requests.Carts;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Carts;

public class CartFilterRequestValidator : BaseGridValidator<CartFilterRequest>
{
    public CartFilterRequestValidator() : base()
    {
    }
}
