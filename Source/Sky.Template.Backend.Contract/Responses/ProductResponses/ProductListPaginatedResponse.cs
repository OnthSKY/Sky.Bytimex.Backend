using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.ProductResponses;

public class ProductListPaginatedResponse
{
    public PaginatedData<SingleProductResponse> Products { get; set; } = new();
}
