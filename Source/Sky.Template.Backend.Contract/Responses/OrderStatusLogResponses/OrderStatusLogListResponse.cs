namespace Sky.Template.Backend.Contract.Responses.OrderStatusLogResponses;

public class OrderStatusLogListResponse
{
    public List<OrderStatusLogResponse> Logs { get; set; } = new();
}
