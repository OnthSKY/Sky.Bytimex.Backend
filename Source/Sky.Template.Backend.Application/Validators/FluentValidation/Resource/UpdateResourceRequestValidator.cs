using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Resources;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Resource;

public class UpdateResourceRequestValidator : AbstractValidator<UpdateResourceRequest>
{
    public UpdateResourceRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}
