namespace Sky.Template.Backend.Contract.Requests.CartItems;

public class CreateCartItemRequest
{
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Currency { get; set; }
}
