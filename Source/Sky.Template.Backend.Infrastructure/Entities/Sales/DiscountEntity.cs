using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Sales;

[TableName("discounts")]
public class DiscountEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("code")]
    public string Code { get; set; } = string.Empty;

    [DbManager.mColumn("description")]
    public string? Description { get; set; }

    [DbManager.mColumn("discount_type")]
    public string DiscountType { get; set; } = "PERCENTAGE";

    [DbManager.mColumn("value")]
    public decimal Value { get; set; }

    [DbManager.mColumn("start_date")]
    public DateTime StartDate { get; set; }

    [DbManager.mColumn("end_date")]
    public DateTime EndDate { get; set; }

    [DbManager.mColumn("usage_limit")]
    public int? UsageLimit { get; set; }

    [DbManager.mColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [DbManager.mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [DbManager.mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    [DbManager.mColumn("delete_reason")]
    public string? DeleteReason { get; set; }
}
