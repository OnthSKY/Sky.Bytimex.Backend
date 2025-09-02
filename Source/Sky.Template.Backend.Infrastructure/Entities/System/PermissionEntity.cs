using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.System;

[TableName("permissions")]
public class PermissionEntity : BaseEntity<int>, ISoftDeletable
{
    [DbManager.mColumn("code")]
    public string Code { get; set; } = string.Empty;

    [DbManager.mColumn("name")]
    public string Name { get; set; } = string.Empty;

    [DbManager.mColumn("description")]
    public string? Description { get; set; }

    [DbManager.mColumn("resource_id")]
    public int? ResourceId { get; set; }

    [DbManager.mColumn("action")]
    public string Action { get; set; } = string.Empty;

    [DbManager.mColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [DbManager.mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [DbManager.mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    [DbManager.mColumn("delete_reason")]
    public string? DeleteReason { get; set; }
} 