using Sky.Template.Backend.Infrastructure.Entities.Base;
using static Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.DbManager;

namespace Sky.Template.Backend.Infrastructure.Entities.Sales;

public class CartItemLocalizedJoinEntity : BaseEntity<Guid>, ISoftDeletable
{
    [mColumn("cart_id")]
    public Guid CartId { get; set; }

    [mColumn("product_id")]
    public Guid ProductId { get; set; }

    [mColumn("quantity")]
    public decimal Quantity { get; set; }

    [mColumn("unit_price")]
    public decimal UnitPrice { get; set; }

    [mColumn("currency")]
    public string Currency { get; set; } = string.Empty;

    [mColumn("status")]
    public string Status { get; set; } = string.Empty;

    [mColumn("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [mColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    [mColumn("delete_reason")]
    public string? DeleteReason { get; set; }
}
