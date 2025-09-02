namespace Sky.Template.Backend.Infrastructure.Configs.Products;

public static class ProductCategoryGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "name", "pc.name" },
        { "parentCategoryId", "pc.parent_category_id" },
        { "description", "pc.description" },
        { "createdAt", "pc.created_at" }
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
