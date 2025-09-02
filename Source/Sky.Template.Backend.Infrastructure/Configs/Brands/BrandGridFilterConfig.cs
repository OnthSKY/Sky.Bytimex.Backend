using System;
using System.Collections.Generic;

namespace Sky.Template.Backend.Infrastructure.Configs.Brands;

public static class BrandGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        {"slug", "b.slug"},
        {"name", "bt.name"},
        {"status", "b.status"}
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "slug",
        "name"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "b.slug",
        "bt.name"
    };

    public static string GetDefaultOrder() => "b.slug ASC";
}
