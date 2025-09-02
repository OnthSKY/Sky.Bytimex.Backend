using Sky.Template.Backend.Infrastructure.Entities.Base;
using static Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.DbManager;

namespace Sky.Template.Backend.Infrastructure.Entities.Product;

public class ProductLocalizedJoinEntity : BaseEntity<Guid>, ISoftDeletable
{
    [mColumn("vendor_id")]
    public Guid VendorId { get; set; }

    [mColumn("slug")]
    public string Slug { get; set; } = string.Empty;

    [mColumn("name")]
    public string Name { get; set; } = string.Empty;
    [mColumn("barcode")]
    public string Barcode { get; set; } = string.Empty;

    [mColumn("description")]
    public string? Description { get; set; }


    [mColumn("category_id")]
    public Guid? CategoryId { get; set; }

    [mColumn("product_type")]
    public string ProductType { get; set; } = string.Empty;
    [mColumn("is_decimal_quantity_allowed")]
    public bool IsDecimalQuantityAllowed { get; set; } 

    [mColumn("unit")]
    public string Unit { get; set; }
    [mColumn("price")]
    public decimal Price { get; set; }

    [mColumn("stock_quantity")]
    public decimal? StockQuantity { get; set; }

    [mColumn("is_stock_tracked")]
    public bool IsStockTracked { get; set; }

    [mColumn("sku")]
    public string? Sku { get; set; }

    [mColumn("status")]
    public string Status { get; set; } = string.Empty;

    [mColumn("primary_image_url")]
    public string? PrimaryImageUrl { get; set; }

    [mColumn("is_deleted")]
    public bool IsDeleted { get; set; }
    [mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }
    [mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }
    [mColumn("delete_reason")]
    public string? DeleteReason { get; set; }
}
