namespace Sky.Template.Backend.Contract.Requests.Roles;

public class UpdateUserRoleRequest
{
    public Guid UserId { get; set; }
    public int RoleId { get; set; }
}
