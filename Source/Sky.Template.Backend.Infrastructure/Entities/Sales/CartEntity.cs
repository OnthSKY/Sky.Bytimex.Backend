using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Sales;

[TableName("carts")]
public class CartEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("buyer_id")]
    public Guid BuyerId { get; set; }

    [DbManager.mColumn("status")]
    public string Status { get; set; } = "OPEN";

    [DbManager.mColumn("coupon_code")]
    public string? CouponCode { get; set; }

    [DbManager.mColumn("currency")]
    public string Currency { get; set; } = string.Empty;

    [DbManager.mColumn("total_price")]
    public decimal TotalPrice { get; set; }

    [DbManager.mColumn("note")]
    public string? Note { get; set; }

    [DbManager.mColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [DbManager.mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [DbManager.mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    [DbManager.mColumn("delete_reason")]
    public string? DeleteReason { get; set; }
}
