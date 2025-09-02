using Sky.Template.Backend.Contract.Requests;

namespace Sky.Template.Backend.Contract.Requests.Products;

public class UpdateProductRequest : BaseRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public string ProductType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? StockQuantity { get; set; }
    public bool IsStockTracked { get; set; }
    public string? Sku { get; set; }
    public string Status { get; set; } = "ACTIVE";
} 