
namespace Sky.Template.Backend.Contract.Responses.Dashboard.User;

public class UserPurchaseSummaryResponse
{
    public int TotalPurchases { get; set; }
    public decimal TotalSpent { get; set; }
    public int UsedCoupons { get; set; }
}
