using System.ComponentModel.DataAnnotations.Schema;
using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Sales;

[TableName("returns")]
public class ReturnEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("order_id")]
    public Guid OrderId { get; set; }

    [DbManager.mColumn("order_detail_id")]
    public Guid? OrderDetailId { get; set; }

    [DbManager.mColumn("buyer_id")]
    public Guid BuyerId { get; set; }

    [DbManager.mColumn("reason")]
    public string Reason { get; set; } = string.Empty;

    [DbManager.mColumn("status")]
    public string Status { get; set; } = "PENDING";

    [DbManager.mColumn("processed_at")]
    public DateTime? ProcessedAt { get; set; }

    [DbManager.mColumn("processed_by")]
    public Guid? ProcessedBy { get; set; }

    [DbManager.mColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [DbManager.mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [DbManager.mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    [DbManager.mColumn("delete_reason")]
    public string? DeleteReason { get; set; }
}
