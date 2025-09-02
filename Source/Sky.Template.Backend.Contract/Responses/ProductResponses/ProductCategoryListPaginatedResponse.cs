using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.ProductResponses;

public class ProductCategoryListPaginatedResponse
{
    public PaginatedData<SingleProductCategoryResponse> Categories { get; set; } = new();
}
