using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Sales;

[TableName("cart_items")]
public class CartItemEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("cart_id")]
    public Guid CartId { get; set; }

    [DbManager.mColumn("product_id")]
    public Guid ProductId { get; set; }

    [DbManager.mColumn("quantity")]
    public decimal Quantity { get; set; }

    [DbManager.mColumn("unit_price")]
    public decimal UnitPrice { get; set; }

    [DbManager.mColumn("currency")]
    public string Currency { get; set; } = string.Empty;

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
