using Sky.Template.Backend.Core.Requests.Base;

namespace Sky.Template.Backend.Contract.Requests.StockMovements;

public class StockMovementFilterRequest : GridRequest
{
    public Guid? ProductId { get; set; }
    public string? MovementType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
