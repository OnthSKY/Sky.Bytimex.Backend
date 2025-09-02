using FluentValidation;
using Sky.Template.Backend.Contract.Requests.StockMovements;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.StockMovements;

public class StockMovementFilterRequestValidator : BaseGridValidator<StockMovementFilterRequest>
{
    public StockMovementFilterRequestValidator() : base()
    {
        RuleFor(x => x.StartDate).LessThanOrEqualTo(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage(SharedResourceKeys.InvalidDate);
    }
}
