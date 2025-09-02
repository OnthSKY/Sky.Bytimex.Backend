namespace Sky.Template.Backend.Infrastructure.Configs.Sales;

public static class OrderGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "vendorId", "s.vendor_id" },
        { "buyerId", "s.buyer_id" },
        { "saleStatus", "s.order_status" },
        { "currency", "s.currency" },
        { "minAmount", "s.total_amount >= @minAmount" },
        { "maxAmount", "s.total_amount <= @maxAmount" },
        { "startDate", "s.order_date >= @startDate" },
        { "endDate", "s.order_date <= @endDate" },
        { "createdAt", "s.created_at" }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        // örnek: "saleStatus"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "s.order_status",
        "s.currency"
    };

    public static string GetDefaultOrder() => "s.created_at DESC";
} 