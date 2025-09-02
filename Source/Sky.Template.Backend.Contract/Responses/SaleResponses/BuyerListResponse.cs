 
namespace Sky.Template.Backend.Contract.Responses.SaleResponses;

public class BuyerListResponse 
{
    public List<SingleBuyerResponse> Buyers { get; set; } = new();
    public int TotalCount  => Buyers.Count;
}