using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Auth;

public class RefreshTokenEntity : BaseEntity<Guid>
{
    [DbManager.mColumn("token")]
    public string Token { get; set; } = string.Empty;

    [DbManager.mColumn("user_id")]
    public Guid UserId { get; set; }

    [DbManager.mColumn("expiration_date")]
    public DateTime ExpirationDate { get; set; }

    [DbManager.mColumn("schema_name")]
    public string? SchemaName { get; set; }

    [DbManager.mColumn("created_by_ip")]
    public string? CreatedByIp { get; set; }

    [DbManager.mColumn("revoked_at")]
    public DateTime? RevokedAt { get; set; }

    [DbManager.mColumn("revoked_by_ip")]
    public string? RevokedByIp { get; set; }
}
