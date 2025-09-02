using FluentValidation;
using Sky.Template.Backend.Contract.Requests.StockMovements;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.StockMovements;

public class CreateStockMovementRequestValidator : AbstractValidator<CreateStockMovementRequest>
{
    public CreateStockMovementRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.MovementType).NotEmpty().Must(IsValidType);
        When(x => x.MovementType == "IN", () =>
        {
            RuleFor(x => x.SupplierId).NotEmpty();
        });
        When(x => x.MovementType == "OUT" || x.MovementType == "RETURN", () =>
        {
            RuleFor(x => x.RelatedOrderId).NotEmpty();
        });
    }

    private bool IsValidType(string type)
        => new[] { "IN", "OUT", "RETURN", "CORRECTION" }.Contains(type);
}
