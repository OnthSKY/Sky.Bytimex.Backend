using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.ShipmentResponses;

public class ShipmentListPaginatedResponse : BaseServiceResponse
{
    public PaginatedData<ShipmentResponse> Shipments { get; set; } = new();
}
