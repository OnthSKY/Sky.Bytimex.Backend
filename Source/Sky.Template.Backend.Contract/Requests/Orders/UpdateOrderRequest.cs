
namespace Sky.Template.Backend.Contract.Requests.Orders;

public class UpdateOrderRequest : BaseRequest
{
    public Guid VendorId { get; set; }
    public Guid? BuyerId { get; set; }
    public string? BuyerDescription { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string OrderStatus { get; set; } = "COMPLETED";
    public DateTime OrderDate { get; set; }
} 