using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.CartResponses;

public class CartResponse : BaseServiceResponse
{
    public Guid Id { get; set; }
    public Guid BuyerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CouponCode { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string? Note { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public string? DeleteReason { get; set; }
}
