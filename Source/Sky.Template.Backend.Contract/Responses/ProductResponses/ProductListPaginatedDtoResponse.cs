using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.ProductResponses;

public class ProductListPaginatedDtoResponse
{
    public PaginatedData<ProductDto> Products { get; set; } = new();
}
