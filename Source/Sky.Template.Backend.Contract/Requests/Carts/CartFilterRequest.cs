using Sky.Template.Backend.Core.Requests.Base;

namespace Sky.Template.Backend.Contract.Requests.Carts;

public class CartFilterRequest : GridRequest
{
    public Guid? BuyerId { get; set; }
    public string? Status { get; set; }
}
