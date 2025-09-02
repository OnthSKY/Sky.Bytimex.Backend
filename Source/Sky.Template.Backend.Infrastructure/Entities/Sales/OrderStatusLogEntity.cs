using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Sales;

[TableName("order_status_logs")]
public class OrderStatusLogEntity
{
    [DbManager.mColumn("id")]
    public Guid Id { get; set; }

    [DbManager.mColumn("order_id")]
    public Guid OrderId { get; set; }

    [DbManager.mColumn("old_status")]
    public string OldStatus { get; set; } = string.Empty;

    [DbManager.mColumn("new_status")]
    public string NewStatus { get; set; } = string.Empty;

    [DbManager.mColumn("changed_by")]
    public Guid? ChangedBy { get; set; }

    [DbManager.mColumn("changed_at")]
    public DateTime ChangedAt { get; set; }

    [DbManager.mColumn("note")]
    public string? Note { get; set; }
}
