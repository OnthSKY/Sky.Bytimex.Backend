using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.VendorResponses;

public class PaginatedVendorListResponse
{
    public PaginatedData<VendorDto> Vendors { get; set; } = new();
}
