 
namespace Sky.Template.Backend.Contract.Requests.Products;

public class CreateProductRequest : BaseRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public string ProductType { get; set; } = default!; // PHYSICAL, DIGITAL, SERVICE
    public decimal Price { get; set; }
    public int? StockQuantity { get; set; }
    public bool IsStockTracked { get; set; }
    public string? Sku { get; set; }
    public string Status { get; set; } = "ACTIVE";
}
