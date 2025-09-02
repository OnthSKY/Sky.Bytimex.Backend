using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Vendor;

[TableName("vendor_settings")]
public class VendorSettingEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("vendor_id")]
    public Guid VendorId { get; set; }

    [DbManager.mColumn("key")]
    public string Key { get; set; } = string.Empty;

    [DbManager.mColumn("value")]
    public string Value { get; set; } = string.Empty;

    [DbManager.mColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [DbManager.mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [DbManager.mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    [DbManager.mColumn("delete_reason")]
    public string? DeleteReason { get; set; }
}
