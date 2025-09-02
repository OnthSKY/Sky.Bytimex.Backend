using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.ProductResponses;

public class ProductListResponse : BaseServiceResponse
{
    public List<SingleProductResponse> Products { get; set; } = new();
    public int TotalCount { get; set; }
} 