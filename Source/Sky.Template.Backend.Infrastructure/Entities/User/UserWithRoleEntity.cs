using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.User;

public class UserWithRoleEntity: BaseUserEntity
{
    [DbManager.mColumn("role_id")]
    public int RoleId{ get; set; }
    [DbManager.mColumn("role_name")]
    public string RoleName { get; set; }
    [DbManager.mColumn("role_description")]
    public string RoleDescription { get; set; }
    [DbManager.mColumn("permission_names")]
    public string PermissionNamesRaw { get; set; }

    public List<string> Permissions =>
        PermissionNamesRaw?.Split(',').Select(p => p.Trim()).ToList() ?? new();
}
