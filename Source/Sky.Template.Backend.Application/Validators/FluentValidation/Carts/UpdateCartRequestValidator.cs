using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Carts;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Carts;

public class UpdateCartRequestValidator : AbstractValidator<UpdateCartRequest>
{
    public UpdateCartRequestValidator()
    {
        // Optional fields; no specific rules.
    }
}
