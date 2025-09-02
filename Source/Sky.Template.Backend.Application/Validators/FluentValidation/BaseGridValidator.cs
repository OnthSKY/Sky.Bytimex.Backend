using FluentValidation;
using Sky.Template.Backend.Core.Localization;
using Sky.Template.Backend.Core.Requests.Base;

namespace Sky.Template.Backend.Application.Validators.FluentValidation;
public abstract class BaseGridValidator<T> : AbstractValidator<T> where T : GridRequest
{
    protected BaseGridValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithMessage(SharedResourceKeys.InvalidPage);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage(SharedResourceKeys.InvalidPageSize);
        RuleFor(x => x.OrderDirection)
            .Must(x => x.Equals("ASC", StringComparison.OrdinalIgnoreCase) || x.Equals("DESC", StringComparison.OrdinalIgnoreCase))
            .WithMessage(SharedResourceKeys.OrderDirectionPropNotFound);
    }
}
