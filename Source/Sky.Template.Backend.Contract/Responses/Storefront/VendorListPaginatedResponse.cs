using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.Storefront;

public class VendorListPaginatedResponse
{
    public PaginatedData<VendorListItemResponse> Vendors { get; set; } = new();
}
