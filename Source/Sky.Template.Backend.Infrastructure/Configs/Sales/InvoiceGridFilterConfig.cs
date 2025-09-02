using System.Collections.Generic;
using System;

namespace Sky.Template.Backend.Infrastructure.Configs.Sales;

public static class InvoiceGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        {"buyerId", "i.buyer_id"},
        {"status", "i.status"},
        {"startDate", "i.invoice_date >= @startDate"},
        {"endDate", "i.invoice_date <= @endDate"},
        {"createdAt", "i.created_at"}
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "status"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "i.invoice_number",
        "i.status",
        "i.currency"
    };

    public static string GetDefaultOrder() => "i.created_at DESC";
}
