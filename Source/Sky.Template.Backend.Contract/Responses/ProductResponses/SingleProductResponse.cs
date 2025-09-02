using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.ProductResponses;

public class SingleProductResponse : BaseServiceResponse
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public Guid? VendorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string ProductType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public string? Unit { get; set; }
    public string? Barcode { get; set; }
    public decimal? StockQuantity { get; set; }
    public bool IsStockTracked { get; set; }
    public string? Sku { get; set; }
    public bool IsDecimalQuantityAllowed { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public string? DeleteReason { get; set; }
}