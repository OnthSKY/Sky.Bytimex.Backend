using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.OrderStatusLogResponses;

public class OrderStatusLogResponse : BaseServiceResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public Guid? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Note { get; set; }
}
