namespace Sky.Template.Backend.Infrastructure.Configs.Sales;

public static class CartGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "buyerId", "c.buyer_id" },
        { "status", "c.status" },
        { "currency", "c.currency" },
        { "createdAt", "c.created_at" }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "status",
        "currency",
        "couponCode",
        "note"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "c.status",
        "c.currency",
        "c.coupon_code",
        "c.note"
    };

    public static string GetDefaultOrder() => "c.created_at DESC";
}
