namespace Sky.Template.Backend.Contract.Requests.Discounts;

public class ApplyCouponRequest
{
    public string CouponCode { get; set; } = string.Empty;
    public decimal CartTotal { get; set; }
}
