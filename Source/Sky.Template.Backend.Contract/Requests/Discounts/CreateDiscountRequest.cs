namespace Sky.Template.Backend.Contract.Requests.Discounts;

public class CreateDiscountRequest
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = "PERCENTAGE";
    public decimal Value { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UsageLimit { get; set; }
}
