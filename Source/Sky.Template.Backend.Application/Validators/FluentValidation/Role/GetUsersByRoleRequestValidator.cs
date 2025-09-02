using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Roles;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Role;

public class GetUsersByRoleRequestValidator : BaseGridValidator<GetUsersByRoleRequest>
{
    public GetUsersByRoleRequestValidator() : base()
    {

    }
}
