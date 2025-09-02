using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Products;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Products;

public class AddImageRequestValidator : AbstractValidator<AddImageRequest>
{
    public AddImageRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ImageUrl).NotEmpty().MaximumLength(500);
        RuleFor(x => x.AltText).MaximumLength(200);
    }
}

