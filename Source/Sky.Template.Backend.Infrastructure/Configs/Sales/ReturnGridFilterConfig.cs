using System;
using System.Collections.Generic;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.Sales;

public static class ReturnGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "buyerId",   new ColumnMapping("r.buyer_id",   typeof(Guid)) },
        { "orderId",   new ColumnMapping("r.order_id",   typeof(Guid)) },
        { "status",    new ColumnMapping("r.status",     typeof(string)) },
        { "createdAt", new ColumnMapping("r.created_at", typeof(DateTime)) }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "status"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "r.reason",
        "r.status"
    };

    public static string GetDefaultOrder() => "r.created_at DESC";
}