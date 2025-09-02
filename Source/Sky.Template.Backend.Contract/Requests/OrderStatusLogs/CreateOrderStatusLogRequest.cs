using Sky.Template.Backend.Contract.Requests;

namespace Sky.Template.Backend.Contract.Requests.OrderStatusLogs;

public class CreateOrderStatusLogRequest : BaseRequest
{
    public Guid OrderId { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string? Note { get; set; }
}
