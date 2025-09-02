using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Roles;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Role;

public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(SharedResourceKeys.RoleIdIsRequired);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(SharedResourceKeys.RoleNameIsRequired)
            .MaximumLength(50).WithMessage(SharedResourceKeys.RoleNameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(SharedResourceKeys.DescriptionMaxLength);

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage(SharedResourceKeys.StatusIsRequired)
            .Must(status => status == "ACTIVE" || status == "INACTIVE")
            .WithMessage(SharedResourceKeys.StatusMustBeActiveOrInactive);
    }
}
