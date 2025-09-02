using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.SaleResponses;

public class OrderListResponse : BaseServiceResponse
{
    public List<SingleOrderResponse> Orders { get; set; } = new();
    public int TotalCount { get; set; }
    public decimal TotalAmount { get; set; }
} 