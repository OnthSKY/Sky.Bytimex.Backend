using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.SaleResponses;

public class OrderListPaginatedResponse
{
    public PaginatedData<SingleOrderResponse> Orders { get; set; } = new();
}
