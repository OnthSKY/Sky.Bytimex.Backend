using System;

namespace Sky.Template.Backend.Contract.Requests.Products;

public class CreateVariantRequest
{
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public decimal? StockQuantity { get; set; }
}

