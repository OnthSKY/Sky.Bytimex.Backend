using System.ComponentModel.DataAnnotations.Schema;
using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Sales;

[TableName("shipments")]
public class ShipmentEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("order_id")]
    public Guid OrderId { get; set; }

    [DbManager.mColumn("shipment_date")]
    public DateTime ShipmentDate { get; set; }

    [DbManager.mColumn("carrier")]
    public string Carrier { get; set; } = string.Empty;

    [DbManager.mColumn("tracking_number")]
    public string TrackingNumber { get; set; } = string.Empty;

    [DbManager.mColumn("status")]
    public string Status { get; set; } = string.Empty;

    [DbManager.mColumn("estimated_delivery_date")]
    public DateTime? EstimatedDeliveryDate { get; set; }

    [DbManager.mColumn("notes")]
    public string? Notes { get; set; }

    [DbManager.mColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [DbManager.mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [DbManager.mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    [DbManager.mColumn("delete_reason")]
    public string? DeleteReason { get; set; }
}
