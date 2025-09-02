using System.ComponentModel.DataAnnotations.Schema;
using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Sales;
[TableName("orders")]
public class OrderEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("vendor_id")]
    public Guid VendorId { get; set; }

    [DbManager.mColumn("buyer_id")]
    public Guid? BuyerId { get; set; }

    [DbManager.mColumn("buyer_description")]
    public string? BuyerDescription { get; set; }

    [DbManager.mColumn("total_amount")]
    public decimal TotalAmount { get; set; }

    [DbManager.mColumn("currency")]
    public string Currency { get; set; } = "TRY";

    [DbManager.mColumn("order_status")]
    public string OrderStatus { get; set; } = "PENDING";

    [DbManager.mColumn("discount_code")]
    public string? DiscountCode { get; set; }

    [DbManager.mColumn("discount_amount")]
    public decimal? DiscountAmount { get; set; }

    [DbManager.mColumn("payment_status")]
    public string PaymentStatus { get; set; } = "AWAITING";

    [DbManager.mColumn("order_date")]
    public DateTime OrderDate { get; set; }

    [DbManager.mColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [DbManager.mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [DbManager.mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    [DbManager.mColumn("delete_reason")]
    public string? DeleteReason { get; set; }
} 