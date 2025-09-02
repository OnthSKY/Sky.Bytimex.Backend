using FluentValidation;
using Sky.Template.Backend.Contract.Requests.BrandRequests;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Brand;

public class UpdateBrandRequestValidator : AbstractValidator<UpdateBrandRequest>
{
    public UpdateBrandRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Validation.Brand.Id.Required");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Validation.Brand.Slug.Required")
            .Matches("^[a-z0-9-]+$").WithMessage("Validation.Brand.Slug.InvalidFormat")
            .MinimumLength(2).MaximumLength(100).WithMessage("Validation.Brand.Slug.Length");

        RuleFor(x => x.Status)
            .Must(s => s == "ACTIVE" || s == "INACTIVE").WithMessage("Validation.Brand.Status.Invalid");

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
