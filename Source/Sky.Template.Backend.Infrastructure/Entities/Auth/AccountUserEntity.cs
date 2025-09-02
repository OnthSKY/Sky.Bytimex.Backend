using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Auth;

public class AccountUserEntity
{
    [DbManager.mColumn("id")]
    public Guid Id { get; set; }

    [DbManager.mColumn("tenant_schema_name")]
    public string SchemaName { get; set; }

    [DbManager.mColumn("email")]
    public string Email { get; set; }

    [DbManager.mColumn("phone")]
    public string Phone { get; set; }

    [DbManager.mColumn("password_hash")]
    public string Password { get; set; }
}
