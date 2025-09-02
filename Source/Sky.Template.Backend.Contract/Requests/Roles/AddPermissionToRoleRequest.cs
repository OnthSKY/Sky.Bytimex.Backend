namespace Sky.Template.Backend.Contract.Requests.Roles;

public class AddPermissionToRoleRequest
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
}
