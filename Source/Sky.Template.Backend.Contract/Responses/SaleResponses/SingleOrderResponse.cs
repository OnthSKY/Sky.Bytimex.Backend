using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.SaleResponses;

public class SingleOrderResponse : BaseServiceResponse
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public string? VendorName { get; set; }
    public Guid? BuyerId { get; set; }
    public string? BuyerName { get; set; }
    public string? BuyerDescription { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public string? DiscountCode { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public string? DeleteReason { get; set; }
}