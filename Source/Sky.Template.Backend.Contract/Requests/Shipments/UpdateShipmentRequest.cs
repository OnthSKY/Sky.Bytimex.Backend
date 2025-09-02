namespace Sky.Template.Backend.Contract.Requests.Shipments;

public class UpdateShipmentRequest
{
    public DateTime? ShipmentDate { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
}
