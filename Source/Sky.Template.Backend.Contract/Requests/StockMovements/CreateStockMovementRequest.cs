using Sky.Template.Backend.Contract.Requests;

namespace Sky.Template.Backend.Contract.Requests.StockMovements;

public class CreateStockMovementRequest : BaseRequest
{
    public Guid ProductId { get; set; }
    public Guid? SupplierId { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public DateTime? MovementDate { get; set; }
    public string? Description { get; set; }
    public Guid? RelatedOrderId { get; set; }
}
