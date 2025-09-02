namespace Sky.Template.Backend.Contract.Requests.Invoices;

public class CreateInvoiceRequest
{
    public Guid OrderId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid BuyerId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string? PdfUrl { get; set; }
    public string Status { get; set; } = "ACTIVE";
}
