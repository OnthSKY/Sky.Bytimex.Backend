using System;

namespace Sky.Template.Backend.Infrastructure.Entities.Product;

public class ProductVariantEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? Sku { get; set; }
    public decimal? Price { get; set; }
    public decimal? StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

