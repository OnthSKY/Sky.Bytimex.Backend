namespace Sky.Template.Backend.Contract.Requests.Payments;

public class UpdatePaymentRequest
{
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public string? PaymentType { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string? TxHash { get; set; }
    public string Status { get; set; } = "ACTIVE";
}
