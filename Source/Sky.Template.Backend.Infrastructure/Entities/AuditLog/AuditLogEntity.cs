using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.AuditLog;

public class AuditLogEntity : BaseEntity<Guid>
{
    [DbManager.mColumn("user_id")]
    public Guid? UserId { get; set; }

    [DbManager.mColumn("action")]
    public string Action { get; set; } = string.Empty;

    [DbManager.mColumn("table_name")]
    public string? TableName { get; set; }

    [DbManager.mColumn("record_id")]
    public string? RecordId { get; set; }

    [DbManager.mColumn("old_values")]
    public string? OldValues { get; set; }

    [DbManager.mColumn("new_values")]
    public string? NewValues { get; set; }
}
