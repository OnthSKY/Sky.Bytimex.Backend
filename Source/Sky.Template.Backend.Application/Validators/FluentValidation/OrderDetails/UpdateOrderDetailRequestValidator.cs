using FluentValidation;
using Sky.Template.Backend.Contract.Requests.OrderDetails;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.OrderDetails;

public class UpdateOrderDetailRequestValidator : AbstractValidator<UpdateOrderDetailRequest>
{
    public UpdateOrderDetailRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThan(0);
    }
}

