using System;
using System.Collections.Generic;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.Sales;

public static class OrderGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "vendorId",   new ColumnMapping("s.vendor_id",        typeof(Guid)) },
        { "buyerId",    new ColumnMapping("s.buyer_id",         typeof(Guid)) },
        { "saleStatus", new ColumnMapping("s.order_status",     typeof(string)) },
        { "currency",   new ColumnMapping("s.currency",         typeof(string)) },
        { "minAmount",  new ColumnMapping("s.total_amount >= @minAmount", typeof(decimal)) },
        { "maxAmount",  new ColumnMapping("s.total_amount <= @maxAmount", typeof(decimal)) },
        { "startDate",  new ColumnMapping("s.order_date >= @startDate",   typeof(DateTime)) },
        { "endDate",    new ColumnMapping("s.order_date <= @endDate",     typeof(DateTime)) },
        { "createdAt",  new ColumnMapping("s.created_at",       typeof(DateTime)) }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        // örnek: "saleStatus" (istenirse LIKE ile aranabilir)
    };

    public static List<string> GetSearchColumns() => new()
    {
        "s.order_status",
        "s.currency"
    };

    public static string GetDefaultOrder() => "s.created_at DESC";
}