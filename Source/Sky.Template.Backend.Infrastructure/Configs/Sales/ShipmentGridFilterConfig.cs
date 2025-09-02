using System.Collections.Generic;

namespace Sky.Template.Backend.Infrastructure.Configs.Sales;

public static class ShipmentGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        {"orderId", "s.order_id"},
        {"carrier", "s.carrier"},
        {"status", "s.status"},
        {"trackingNumber", "s.tracking_number"},
        {"startDate", "s.shipment_date >= @startDate"},
        {"endDate", "s.shipment_date <= @endDate"},
        {"createdAt", "s.created_at"}
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "carrier",
        "status",
        "trackingNumber"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "s.carrier",
        "s.tracking_number",
        "s.status"
    };

    public static string GetDefaultOrder() => "s.created_at DESC";
}
