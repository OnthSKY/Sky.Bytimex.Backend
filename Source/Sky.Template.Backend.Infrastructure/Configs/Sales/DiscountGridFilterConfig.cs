using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.Sales;

public static class DiscountGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "code",       new ColumnMapping("d.code", typeof(string)) },
        { "startDate",  new ColumnMapping("d.start_date >= @startDate", typeof(DateTime)) },
        { "endDate",    new ColumnMapping("d.end_date <= @endDate",   typeof(DateTime)) },
        { "createdAt",  new ColumnMapping("d.created_at", typeof(DateTime)) }
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