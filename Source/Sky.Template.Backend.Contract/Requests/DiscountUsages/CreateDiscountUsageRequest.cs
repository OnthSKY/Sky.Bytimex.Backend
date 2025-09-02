namespace Sky.Template.Backend.Contract.Requests.DiscountUsages;

public class CreateDiscountUsageRequest
{
    public Guid DiscountId { get; set; }
    public Guid? BuyerId { get; set; }
    public Guid? OrderId { get; set; }
}
