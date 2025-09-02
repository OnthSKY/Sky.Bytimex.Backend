using Sky.Template.Backend.Contract.Requests;

namespace Sky.Template.Backend.Contract.Requests.OrderDetails;

public class CreateOrderDetailRequest : BaseRequest
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Status { get; set; } = "ACTIVE";
}

