using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.ProductResponses;

public class ProductVariantListPaginatedResponse
{
    public PaginatedData<ProductVariantDto> Variants { get; set; } = new();
}

