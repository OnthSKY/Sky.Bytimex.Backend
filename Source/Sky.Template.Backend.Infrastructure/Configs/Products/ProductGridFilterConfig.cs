using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.Products;

public static class ProductGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "id",           new ColumnMapping("p.id",            typeof(Guid)) },
        { "vendorId",     new ColumnMapping("p.vendor_id",     typeof(Guid)) },
        { "name",         new ColumnMapping("COALESCE(pt_lang.name, pt_any.name)", typeof(string)) },
        { "sku",          new ColumnMapping("p.sku",           typeof(string)) },
        { "status",       new ColumnMapping("p.status",        typeof(string)) },
        { "categoryId",   new ColumnMapping("p.category_id",   typeof(Guid)) },
        { "minPrice",     new ColumnMapping("p.price >= @minPrice", typeof(decimal)) }, // hazır koşul
        { "maxPrice",     new ColumnMapping("p.price <= @maxPrice", typeof(decimal)) }, // hazır koşul
        { "createdAt",    new ColumnMapping("p.created_at",    typeof(DateTime)) },
        { "slug",         new ColumnMapping("p.slug",          typeof(string)) }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "name","sku"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "pt_lang.name","pt_any.name","p.slug","p.sku","p.barcode","pt_lang.description","pt_any.description"
    };

    public static string GetDefaultOrder() => "p.created_at DESC";
}
