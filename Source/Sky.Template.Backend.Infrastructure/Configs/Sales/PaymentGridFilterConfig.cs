using System.Collections.Generic;

namespace Sky.Template.Backend.Infrastructure.Configs.Sales;

public static class PaymentGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        {"orderId", "p.order_id"},
        {"buyerId", "p.buyer_id"},
        {"paymentType", "p.payment_type"},
        {"paymentStatus", "p.payment_status"},
        {"createdAt", "p.created_at"}
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "paymentType",
        "paymentStatus"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "p.payment_type",
        "p.payment_status",
        "p.currency"
    };

    public static string GetDefaultOrder() => "p.created_at DESC";
}
