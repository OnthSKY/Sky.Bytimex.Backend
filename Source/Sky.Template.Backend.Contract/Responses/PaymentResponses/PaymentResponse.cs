using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.PaymentResponses;

public class PaymentResponse : BaseServiceResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? TxHash { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
