using System;
using System.Collections.Generic;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.Sales;

public static class CartGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "buyerId",   new ColumnMapping("c.buyer_id",   typeof(Guid)) },
        { "status",    new ColumnMapping("c.status",     typeof(string)) },
        { "currency",  new ColumnMapping("c.currency",   typeof(string)) },
        { "couponCode",new ColumnMapping("c.coupon_code",typeof(string)) },
        { "note",      new ColumnMapping("c.note",       typeof(string)) },
        { "createdAt", new ColumnMapping("c.created_at", typeof(DateTime)) }
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