using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Permissions;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Permission;

public class UpdatePermissionRequestValidator : AbstractValidator<UpdatePermissionRequest>
{
    public UpdatePermissionRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(SharedResourceKeys.PermissionIdIsRequired);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(SharedResourceKeys.PermissionNameIsRequired)
            .MaximumLength(50).WithMessage(SharedResourceKeys.PermissionNameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(SharedResourceKeys.DescriptionMaxLength);
    }
}
