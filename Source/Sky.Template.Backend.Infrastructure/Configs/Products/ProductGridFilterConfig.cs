namespace Sky.Template.Backend.Infrastructure.Configs.Products;

public static class ProductGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "id", "p.id" },
        { "vendorId", "p.vendor_id" },
        { "name", "COALESCE(pt_lang.name, pt_any.name)" },
        { "sku", "p.sku" },
        { "status", "p.status" },
        { "categoryId", "p.category_id" },
        { "minPrice", "p.price >= @minPrice" },
        { "maxPrice", "p.price <= @maxPrice" },
        { "createdAt", "p.created_at" },
        { "slug", "p.slug" }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "name",
        "sku"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "pt_lang.name",
        "pt_any.name",
        "p.slug",
        "p.sku",
        "p.barcode",
        "pt_lang.description",
        "pt_any.description"
    };

    public static string GetDefaultOrder() => "p.created_at DESC";
} 