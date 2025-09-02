using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.PaymentMethodResponses;

public class PaymentMethodListPaginatedResponse
{
    public PaginatedData<PaymentMethodResponse> PaymentMethods { get; set; } = default!;
}
