using System;
using System.Collections.Generic;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.Sales;

public static class ShipmentGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "orderId",        new ColumnMapping("s.order_id",       typeof(Guid)) },
        { "carrier",        new ColumnMapping("s.carrier",        typeof(string)) },
        { "status",         new ColumnMapping("s.status",         typeof(string)) },
        { "trackingNumber", new ColumnMapping("s.tracking_number",typeof(string)) },
        { "startDate",      new ColumnMapping("s.shipment_date >= @startDate", typeof(DateTime)) },
        { "endDate",        new ColumnMapping("s.shipment_date <= @endDate",   typeof(DateTime)) },
        { "createdAt",      new ColumnMapping("s.created_at",     typeof(DateTime)) }
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