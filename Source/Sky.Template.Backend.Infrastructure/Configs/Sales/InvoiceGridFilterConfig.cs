using System;
using System.Collections.Generic;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.Sales;

public static class InvoiceGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "buyerId",   new ColumnMapping("i.buyer_id",                typeof(Guid)) },
        { "status",    new ColumnMapping("i.status",                  typeof(string)) },
        { "startDate", new ColumnMapping("i.invoice_date >= @startDate", typeof(DateTime)) },
        { "endDate",   new ColumnMapping("i.invoice_date <= @endDate",   typeof(DateTime)) },
        { "createdAt", new ColumnMapping("i.created_at",              typeof(DateTime)) }
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