using FluentValidation;
using Sky.Template.Backend.Core.Localization;
using Sky.Template.Backend.Contract.Requests.Roles;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Role;

public class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequest>
{
    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage(SharedResourceKeys.RoleIdCannotBeEmpty);

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(SharedResourceKeys.UserIdCannotBeEmpty);
    }
}
