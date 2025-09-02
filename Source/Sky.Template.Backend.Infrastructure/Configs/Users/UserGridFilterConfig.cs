

namespace Sky.Template.Backend.Infrastructure.Configs.Users;

public static class UserGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "firstName", "u.name" },
        { "lastName", "u.surname" },
        { "email", "u.email" },
        { "roleName", "r.name" }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "firstName",
        "lastName",
        "email"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "u.name",
        "u.surname",
        "u.email",
        "r.name"
    };

    public static string GetDefaultOrder() => "u.name ASC";
}
