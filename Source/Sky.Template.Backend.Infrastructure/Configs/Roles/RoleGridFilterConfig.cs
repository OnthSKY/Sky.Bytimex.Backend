namespace Sky.Template.Backend.Infrastructure.Configs.Roles;

public static class RoleGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "name", "r.name" },       
        { "status", "r.status" },
        { "description", "r.description" },
        { "createdAt", "r.created_at" }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "name",
        "description"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "r.name",
        "r.description",
        "r.status"
    };

    public static string GetDefaultOrder() => "r.created_at DESC";
} 