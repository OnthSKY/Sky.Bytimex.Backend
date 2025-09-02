using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.System;

[TableName("payment_methods")]
public class PaymentMethodEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("name")]
    public string Name { get; set; } = string.Empty;

    [DbManager.mColumn("code")]
    public string Code { get; set; } = string.Empty;

    [DbManager.mColumn("description")]
    public string? Description { get; set; }

    [DbManager.mColumn("is_active")]
    public bool IsActive { get; set; }

    [DbManager.mColumn("supported_currency")]
    public string SupportedCurrency { get; set; } = string.Empty;

    [DbManager.mColumn("type")]
    public string Type { get; set; } = string.Empty;

    [DbManager.mColumn("status")]
    public string Status { get; set; } = string.Empty;

    [DbManager.mColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [DbManager.mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [DbManager.mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    [DbManager.mColumn("delete_reason")]
    public string? DeleteReason { get; set; }
}
