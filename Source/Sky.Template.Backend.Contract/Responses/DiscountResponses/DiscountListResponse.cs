using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.DiscountResponses;

public class DiscountListResponse
{
    public PaginatedData<DiscountDto> Discounts { get; set; } = new();
}
