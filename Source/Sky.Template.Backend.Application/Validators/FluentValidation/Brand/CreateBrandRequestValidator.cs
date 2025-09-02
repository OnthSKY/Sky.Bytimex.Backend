using FluentValidation;
using Sky.Template.Backend.Contract.Requests.BrandRequests;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Brand;

public class CreateBrandRequestValidator : AbstractValidator<CreateBrandRequest>
{
    public CreateBrandRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Validation.Brand.Slug.Required")
            .Matches("^[a-z0-9-]+$").WithMessage("Validation.Brand.Slug.InvalidFormat")
            .MinimumLength(2).MaximumLength(100).WithMessage("Validation.Brand.Slug.Length");

        RuleFor(x => x.Translations)
            .NotNull().WithMessage("Validation.Brand.Translations.Required")
            .Must(t => t != null && t.Count > 0).WithMessage("Validation.Brand.Translations.Required");

        RuleForEach(x => x.Translations).ChildRules(trans =>
        {
            trans.RuleFor(t => t.LanguageCode)
                .NotEmpty().WithMessage("Validation.Brand.Translation.LanguageCode.Required");
            trans.RuleFor(t => t.Name)
                .NotEmpty().WithMessage("Validation.Brand.Translation.Name.Required")
                .MaximumLength(200).WithMessage("Validation.Brand.Translation.Name.Length");
        });
    }
}
