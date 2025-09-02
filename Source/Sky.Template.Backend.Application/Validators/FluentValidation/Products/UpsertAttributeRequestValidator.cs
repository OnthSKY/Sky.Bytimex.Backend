using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Products;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Products;

public class UpsertAttributeRequestValidator : AbstractValidator<UpsertAttributeRequest>
{
    public UpsertAttributeRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.AttributeCode).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AttributeName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ValueText).NotEmpty().MaximumLength(150);
    }
}

