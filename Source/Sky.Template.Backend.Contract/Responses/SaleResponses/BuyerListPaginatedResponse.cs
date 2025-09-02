using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.SaleResponses;

public class BuyerListPaginatedResponse
{
    public PaginatedData<SingleBuyerResponse> Buyers { get; set; }
}