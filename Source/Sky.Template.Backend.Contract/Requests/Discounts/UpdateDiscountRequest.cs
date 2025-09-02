namespace Sky.Template.Backend.Contract.Requests.Discounts;

public class UpdateDiscountRequest : CreateDiscountRequest
{
    public Guid Id { get; set; }
}
