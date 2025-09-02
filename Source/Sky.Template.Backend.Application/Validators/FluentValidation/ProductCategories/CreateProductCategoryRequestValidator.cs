using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Products;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.ProductCategories;

public class CreateProductCategoryRequestValidator : AbstractValidator<CreateProductCategoryRequest>
{
    public CreateProductCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(SharedResourceKeys.Required)
            .MaximumLength(100).WithMessage(SharedResourceKeys.MaxLength);
    }
}
