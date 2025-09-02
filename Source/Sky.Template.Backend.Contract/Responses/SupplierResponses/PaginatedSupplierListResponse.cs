using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.SupplierResponses;

public class PaginatedSupplierListResponse
{
    public PaginatedData<SupplierDto> Suppliers { get; set; } = new();
}