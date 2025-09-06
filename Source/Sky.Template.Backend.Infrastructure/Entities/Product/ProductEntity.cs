using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Core.Localization;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Product;
[Translatable("sys.product_translations", "product_id", "language_code", "name", "description")]
[TableName("products")]
public class ProductEntity : BaseEntity<Guid>, ISoftDeletable
{

    [DbManager.mColumn("vendor_id")]
    public Guid? VendorId { get; set; }

    [DbManager.mColumn("category_id")]
    public Guid? CategoryId { get; set; }

    [DbManager.mColumn("product_type")]
    public string ProductType { get; set; } = string.Empty;

    [DbManager.mColumn("price")]
    public decimal Price { get; set; }

    [DbManager.mColumn("unit")]
    public string? Unit { get; set; }

    [DbManager.mColumn("barcode")]
    public string? Barcode { get; set; }

    [DbManager.mColumn("stock_quantity")]
    public decimal? StockQuantity { get; set; }

    [DbManager.mColumn("is_stock_tracked")]
    public bool IsStockTracked { get; set; }

    [DbManager.mColumn("sku")]
    public string? Sku { get; set; }

    [DbManager.mColumn("slug")]
    public string? Slug { get; set; }

    [DbManager.mColumn("is_decimal_quantity_allowed")]
    public bool IsDecimalQuantityAllowed { get; set; }

    [DbManager.mColumn("status")]
    public string Status { get; set; } = "ACTIVE";

    [DbManager.mColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [DbManager.mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [DbManager.mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    [DbManager.mColumn("delete_reason")]
    public string? DeleteReason { get; set; }
} 