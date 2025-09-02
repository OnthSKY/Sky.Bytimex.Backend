using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Orders;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Sales;

public class UpdateOrderRequestValidator : AbstractValidator<UpdateOrderRequest>
{
    public UpdateOrderRequestValidator()
    {
        RuleFor(x => x.VendorId)
            .NotEmpty().WithMessage(SharedResourceKeys.InvalidGuid);
        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage(SharedResourceKeys.InvalidPrice);
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage(SharedResourceKeys.Required)
            .Length(3).WithMessage(SharedResourceKeys.InvalidCurrency);
        RuleFor(x => x.OrderStatus)
            .NotEmpty().WithMessage(SharedResourceKeys.Required);
    }
}
