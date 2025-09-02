using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Products;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Products;

public class SetVariantAttributeRequestValidator : AbstractValidator<SetVariantAttributeRequest>
{
    public SetVariantAttributeRequestValidator()
    {
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.AttributeCode).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ValueText).NotEmpty().MaximumLength(150);
    }
}

