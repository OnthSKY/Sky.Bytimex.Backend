using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Sales;

[TableName("payments")]
public class PaymentEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("order_id")]
    public Guid OrderId { get; set; }

    [DbManager.mColumn("buyer_id")]
    public Guid BuyerId { get; set; }

    [DbManager.mColumn("amount")]
    public decimal Amount { get; set; }

    [DbManager.mColumn("currency")]
    public string Currency { get; set; } = "TRY";

    [DbManager.mColumn("payment_type")]
    public string PaymentType { get; set; } = string.Empty;

    [DbManager.mColumn("payment_status")]
    public string PaymentStatus { get; set; } = "PENDING";

    [DbManager.mColumn("tx_hash")]
    public string? TxHash { get; set; }

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
