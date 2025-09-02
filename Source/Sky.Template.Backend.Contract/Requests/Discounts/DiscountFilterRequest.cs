using Sky.Template.Backend.Core.Requests.Base;

namespace Sky.Template.Backend.Contract.Requests.Discounts;

public class DiscountFilterRequest : GridRequest
{
    public string? Code { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
