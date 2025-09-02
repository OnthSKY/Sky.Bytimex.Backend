using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.StockMovementResponses;

public class StockMovementResponse : BaseServiceResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? SupplierId { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public DateTime MovementDate { get; set; }
    public string? Description { get; set; }
    public Guid? RelatedOrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
