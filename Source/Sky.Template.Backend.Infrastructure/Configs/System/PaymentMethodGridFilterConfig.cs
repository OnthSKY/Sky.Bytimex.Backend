using System;
using System.Collections.Generic;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.System;

public static class PaymentMethodGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "code",              new ColumnMapping("pm.code",               typeof(string)) },
        { "type",              new ColumnMapping("pm.type",               typeof(string)) },
        { "status",            new ColumnMapping("pm.status",             typeof(string)) },
        { "supportedCurrency", new ColumnMapping("pm.supported_currency", typeof(string)) },
        { "isActive",          new ColumnMapping("pm.is_active",          typeof(bool)) },
        { "createdAt",         new ColumnMapping("pm.created_at",         typeof(DateTime)) }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "code",
        "type",
        "status",
        "supportedCurrency"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "pm.code",
        "pm.type",
        "pm.status",
        "pm.supported_currency"
    };

    public static string GetDefaultOrder() => "pm.created_at DESC";
}