using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Sky.Template.Backend.Core.Attributes;

namespace Sky.Template.Backend.Infrastructure.Entities.Sales;

[TableName("orders_details")]
public class OrderDetailEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("order_id")]
    public Guid OrderId { get; set; }

    [DbManager.mColumn("product_id")]
    public Guid ProductId { get; set; }

    [DbManager.mColumn("quantity")]
    public decimal Quantity { get; set; }

    [DbManager.mColumn("unit_price")]
    public decimal UnitPrice { get; set; }

    [DbManager.mColumn("total_price")]
    public decimal TotalPrice { get; set; }

    [DbManager.mColumn("status")]
    public string Status { get; set; } = "ACTIVE";

    [DbManager.mColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [DbManager.mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [DbManager.mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    [DbManager.mColumn("delete_reason")]
    public string? DeleteReason { get; set; }
} 