namespace Sky.Template.Backend.Infrastructure.Configs.Sales;

public static class DiscountUsageGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "discountId", "du.discount_id" },
        { "buyerId", "du.buyer_id" },
        { "orderId", "du.order_id" },
        { "createdAt", "du.created_at" }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase);

    public static List<string> GetSearchColumns() => new()
    {
        "du.discount_id::text",
        "du.buyer_id::text",
        "du.order_id::text"
    };

    public static string GetDefaultOrder() => "du.created_at DESC";
}
