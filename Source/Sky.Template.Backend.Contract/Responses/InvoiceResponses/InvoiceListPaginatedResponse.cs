using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.InvoiceResponses;

public class InvoiceListPaginatedResponse : BaseServiceResponse
{
    public PaginatedData<InvoiceResponse> Invoices { get; set; } = new();
}
