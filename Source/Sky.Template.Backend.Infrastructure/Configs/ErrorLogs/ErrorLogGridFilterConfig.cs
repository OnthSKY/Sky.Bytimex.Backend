namespace Sky.Template.Backend.Infrastructure.Configs.ErrorLogs;

public static class ErrorLogGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new()
    {
        {"message", "el.message"},
        {"source", "el.source"},
        {"path", "el.path"},
        {"method", "el.method"},
        {"createdAt", "el.created_at"}
    };

    public static string GetDefaultOrder() => "el.created_at DESC";

    public static HashSet<string> GetLikeFilterKeys() => new() { "message", "source", "path", "method" };

    public static List<string> GetSearchColumns() => new() { "el.message", "el.source", "el.path", "el.method" };
}
