using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.ProductResponses;

public class ProductCategoryListResponse : BaseServiceResponse
{
    public List<SingleProductCategoryResponse> Categories { get; set; } = new();
    public int TotalCount { get; set; }
} 