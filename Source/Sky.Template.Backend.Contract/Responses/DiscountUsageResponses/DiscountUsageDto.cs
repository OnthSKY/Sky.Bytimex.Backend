namespace Sky.Template.Backend.Contract.Responses.DiscountUsageResponses;

public class DiscountUsageDto
{
    public Guid Id { get; set; }
    public Guid DiscountId { get; set; }
    public Guid? BuyerId { get; set; }
    public Guid? OrderId { get; set; }
    public DateTime UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
}
