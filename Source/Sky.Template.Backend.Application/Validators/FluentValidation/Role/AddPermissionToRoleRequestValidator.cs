using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Roles;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Role;

public class AddPermissionToRoleRequestValidator : AbstractValidator<AddPermissionToRoleRequest>
{
    public AddPermissionToRoleRequestValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty().WithMessage(SharedResourceKeys.RoleIdCannotBeEmpty);
        RuleFor(x => x.PermissionId).NotEmpty().WithMessage(SharedResourceKeys.PermissionIdCannotBeEmpty);
    }
}
