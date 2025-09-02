using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.PaymentResponses;

public class PaymentListPaginatedResponse : BaseServiceResponse
{
    public PaginatedData<PaymentResponse> Payments { get; set; } = new();
}
