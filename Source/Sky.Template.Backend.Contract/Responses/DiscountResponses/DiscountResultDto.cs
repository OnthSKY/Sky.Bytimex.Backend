namespace Sky.Template.Backend.Contract.Responses.DiscountResponses;

public class DiscountResultDto
{
    public bool IsValid { get; set; }
    public decimal DiscountAmount { get; set; }
    public string Message { get; set; } = string.Empty;
}
