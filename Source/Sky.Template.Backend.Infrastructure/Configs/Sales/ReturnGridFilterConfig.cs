namespace Sky.Template.Backend.Infrastructure.Configs.Sales;

public static class ReturnGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "buyerId", "r.buyer_id" },
        { "orderId", "r.order_id" },
        { "status", "r.status" },
        { "createdAt", "r.created_at" }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "status" }
    };

    public static List<string> GetSearchColumns() => new()
    {
        "r.reason",
        "r.status"
    };

    public static string GetDefaultOrder() => "r.created_at DESC";
}

