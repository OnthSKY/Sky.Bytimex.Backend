namespace Sky.Template.Backend.Infrastructure.Configs.Permissions;

public static class PermissionGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "name", "p.name" },
        { "description", "p.description" },
        { "createdAt", "p.created_at" }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "name",
        "description"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "p.name",
        "p.description"
    };

    public static string GetDefaultOrder() => "p.created_at DESC";
} 