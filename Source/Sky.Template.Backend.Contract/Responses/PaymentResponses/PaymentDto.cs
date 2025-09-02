namespace Sky.Template.Backend.Contract.Responses.PaymentResponses;

public class PaymentDto
{
    public Guid PaymentId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

