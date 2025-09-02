namespace Sky.Template.Backend.Contract.Requests.Payments;

public class CreatePaymentRequest
{
    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string PaymentType { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = "PENDING";
    public string? TxHash { get; set; }
    public string Status { get; set; } = "ACTIVE";
}
