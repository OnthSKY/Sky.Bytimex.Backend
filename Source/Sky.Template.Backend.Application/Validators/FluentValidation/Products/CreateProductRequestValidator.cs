using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Products;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Products;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.ProductType)
            .NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage(SharedResourceKeys.InvalidPrice);
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage(SharedResourceKeys.Required)
            .Must(s => s == "ACTIVE" || s == "INACTIVE").WithMessage(SharedResourceKeys.InvalidStatus);
    }
}
