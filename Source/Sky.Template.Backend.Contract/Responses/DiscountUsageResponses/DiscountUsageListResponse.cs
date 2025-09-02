using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.DiscountUsageResponses;

public class DiscountUsageListResponse
{
    public PaginatedData<DiscountUsageDto> DiscountUsages { get; set; } = new();
}
