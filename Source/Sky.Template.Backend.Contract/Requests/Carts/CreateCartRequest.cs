namespace Sky.Template.Backend.Contract.Requests.Carts;

public class CreateCartRequest
{
    public Guid BuyerId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? CouponCode { get; set; }
    public string? Note { get; set; }
}
