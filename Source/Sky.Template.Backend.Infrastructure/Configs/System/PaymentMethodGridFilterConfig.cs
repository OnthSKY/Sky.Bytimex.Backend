using System.Collections.Generic;

namespace Sky.Template.Backend.Infrastructure.Configs.System;

public static class PaymentMethodGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        {"code", "pm.code"},
        {"type", "pm.type"},
        {"status", "pm.status"},
        {"supportedCurrency", "pm.supported_currency"},
        {"isActive", "pm.is_active"},
        {"createdAt", "pm.created_at"}
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
