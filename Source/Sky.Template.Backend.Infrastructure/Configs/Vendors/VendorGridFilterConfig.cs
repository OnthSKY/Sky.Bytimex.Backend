namespace Sky.Template.Backend.Infrastructure.Configs.Vendors;

public static class VendorGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "name", "v.name" },
        { "email", "v.email" },
        { "phone", "v.phone" },
        { "status", "v.status" },
        { "createdAt", "v.created_at" }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "name",
        "email",
        "phone"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "v.name",
        "v.email",
        "v.phone"
    };

    public static string GetDefaultOrder() => "v.created_at DESC";
}
