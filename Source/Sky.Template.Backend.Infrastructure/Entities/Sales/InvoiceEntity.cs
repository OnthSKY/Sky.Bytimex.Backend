using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Sales;

[TableName("invoices")]
public class InvoiceEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("order_id")]
    public Guid OrderId { get; set; }

    [DbManager.mColumn("invoice_number")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [DbManager.mColumn("invoice_date")]
    public DateTime InvoiceDate { get; set; }

    [DbManager.mColumn("due_date")]
    public DateTime? DueDate { get; set; }

    [DbManager.mColumn("buyer_id")]
    public Guid BuyerId { get; set; }

    [DbManager.mColumn("total_amount")]
    public decimal TotalAmount { get; set; }

    [DbManager.mColumn("currency")]
    public string Currency { get; set; } = "TRY";

    [DbManager.mColumn("pdf_url")]
    public string? PdfUrl { get; set; }

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
