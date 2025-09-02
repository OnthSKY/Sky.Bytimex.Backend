namespace Sky.Template.Backend.Contract.Requests.CartItems;

public class UpdateCartItemRequest
{
    public Guid? ProductId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Currency { get; set; }
}
