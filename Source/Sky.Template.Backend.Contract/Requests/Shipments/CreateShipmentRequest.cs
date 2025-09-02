namespace Sky.Template.Backend.Contract.Requests.Shipments;

public class CreateShipmentRequest
{
    public Guid OrderId { get; set; }
    public DateTime ShipmentDate { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING";
    public string? Notes { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
}
