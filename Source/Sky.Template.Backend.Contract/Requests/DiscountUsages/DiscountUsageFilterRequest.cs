using Sky.Template.Backend.Core.Requests.Base;

namespace Sky.Template.Backend.Contract.Requests.DiscountUsages;

public class DiscountUsageFilterRequest : GridRequest
{
    public Guid? DiscountId { get; set; }
    public Guid? BuyerId { get; set; }
    public Guid? OrderId { get; set; }
}
