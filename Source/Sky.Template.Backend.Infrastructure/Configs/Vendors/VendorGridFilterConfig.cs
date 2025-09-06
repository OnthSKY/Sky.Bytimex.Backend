using System;
using System.Collections.Generic;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.Vendors;

public static class VendorGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "name",      new ColumnMapping("v.name",      typeof(string)) },
        { "email",     new ColumnMapping("v.email",     typeof(string)) },
        { "phone",     new ColumnMapping("v.phone",     typeof(string)) },
        { "status",    new ColumnMapping("v.status",    typeof(string)) },
        { "createdAt", new ColumnMapping("v.created_at",typeof(DateTime)) }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "name",
        "email",
        "phone"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "v.name",
        "v.email",
        "v.phone"
    };

    public static string GetDefaultOrder() => "v.created_at DESC";
}