using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.StockMovementResponses;

public class StockMovementListResponse
{
    public PaginatedData<StockMovementResponse> StockMovements { get; set; } = new();
}
