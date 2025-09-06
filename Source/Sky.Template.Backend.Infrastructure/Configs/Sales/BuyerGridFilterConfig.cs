using System;
using System.Collections.Generic;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.Sales;

public static class BuyerGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "name",        new ColumnMapping("b.name",         typeof(string)) },
        { "email",       new ColumnMapping("b.email",        typeof(string)) },
        { "phone",       new ColumnMapping("b.phone",        typeof(string)) },
        { "companyName", new ColumnMapping("b.company_name", typeof(string)) },
        { "taxNumber",   new ColumnMapping("b.tax_number",   typeof(string)) },
        { "taxOffice",   new ColumnMapping("b.tax_office",   typeof(string)) },
        { "createdAt",   new ColumnMapping("b.created_at",   typeof(DateTime)) }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "name",
        "email",
        "phone",
        "companyName",
        "taxNumber",
        "taxOffice"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "b.name",
        "b.email",
        "b.phone",
        "b.company_name",
        "b.tax_number",
        "b.tax_office"
    };

    public static string GetDefaultOrder() => "b.created_at DESC";
}