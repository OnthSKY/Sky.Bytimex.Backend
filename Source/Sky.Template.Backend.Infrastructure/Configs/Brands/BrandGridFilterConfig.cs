using System;
using System.Collections.Generic;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.Brands;

public static class BrandGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "slug",   new ColumnMapping("b.slug",  typeof(string)) },
        { "name",   new ColumnMapping("bt.name", typeof(string)) },
        { "status", new ColumnMapping("b.status", typeof(string)) }
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