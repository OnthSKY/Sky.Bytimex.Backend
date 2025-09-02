

using Sky.Template.Backend.Core.Requests.Base;

namespace Sky.Template.Backend.Contract.Requests.Roles;

public class GetUsersByRoleRequest : GridRequest
{
    public int RoleId { get; set; }
}
