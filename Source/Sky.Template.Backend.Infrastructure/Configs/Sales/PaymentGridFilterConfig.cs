using System;
using System.Collections.Generic;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.Sales;

public static class PaymentGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "orderId",       new ColumnMapping("p.order_id",       typeof(Guid)) },
        { "buyerId",       new ColumnMapping("p.buyer_id",       typeof(Guid)) },
        { "paymentType",   new ColumnMapping("p.payment_type",   typeof(string)) },
        { "paymentStatus", new ColumnMapping("p.payment_status", typeof(string)) },
        { "createdAt",     new ColumnMapping("p.created_at",     typeof(DateTime)) }
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