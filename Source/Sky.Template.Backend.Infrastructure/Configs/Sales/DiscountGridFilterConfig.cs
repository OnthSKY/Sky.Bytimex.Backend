namespace Sky.Template.Backend.Infrastructure.Configs.Sales;

public static class DiscountGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "code", "d.code" },
        { "startDate", "d.start_date >= @startDate" },
        { "endDate", "d.end_date <= @endDate" },
        { "createdAt", "d.created_at" }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "code"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "d.code",
        "d.description"
    };

    public static string GetDefaultOrder() => "d.created_at DESC";
}
