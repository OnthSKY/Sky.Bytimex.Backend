using System.ComponentModel.DataAnnotations.Schema;
using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Stock;

[TableName("stock_movements")]
public class StockMovementEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("product_id")]
    public Guid ProductId { get; set; }

    [DbManager.mColumn("supplier_id")]
    public Guid? SupplierId { get; set; }

    [DbManager.mColumn("movement_type")]
    public string MovementType { get; set; } = string.Empty;

    [DbManager.mColumn("quantity")]
    public decimal Quantity { get; set; }

    [DbManager.mColumn("movement_date")]
    public DateTime MovementDate { get; set; }

    [DbManager.mColumn("description")]
    public string? Description { get; set; }

    [DbManager.mColumn("related_order_id")]
    public Guid? RelatedOrderId { get; set; }

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
