using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.System;

[TableName("system_settings")]
public class SystemSettingEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("key")]
    public string Key { get; set; } = string.Empty;

    [DbManager.mColumn("group_name")]
    public string Group { get; set; } = string.Empty;

    [DbManager.mColumn("value")]
    public string Value { get; set; } = string.Empty;

    [DbManager.mColumn("description")]
    public string? Description { get; set; }

    [DbManager.mColumn("is_public")]
    public bool IsPublic { get; set; }

    [DbManager.mColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [DbManager.mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [DbManager.mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    [DbManager.mColumn("delete_reason")]
    public string? DeleteReason { get; set; }
}
