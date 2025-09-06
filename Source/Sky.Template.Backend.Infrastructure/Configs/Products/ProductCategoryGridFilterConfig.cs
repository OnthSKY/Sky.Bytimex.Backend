using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.Products;

public static class ProductCategoryGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "name",             new ColumnMapping("pc.name",              typeof(string)) },
        { "parentCategoryId", new ColumnMapping("pc.parent_category_id",typeof(Guid)) },
        { "description",      new ColumnMapping("pc.description",       typeof(string)) },
        { "createdAt",        new ColumnMapping("pc.created_at",        typeof(DateTime)) }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "name",
        "description"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "pc.name",
        "pc.description"
    };

    public static string GetDefaultOrder() => "pc.created_at DESC";
}