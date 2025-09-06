using System;
using System.Collections.Generic;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.Permissions;

public static class PermissionGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "name",        new ColumnMapping("p.name",        typeof(string)) },
        { "description", new ColumnMapping("p.description", typeof(string)) },
        { "createdAt",   new ColumnMapping("p.created_at",  typeof(DateTime)) }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "name",
        "description"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "p.name",
        "p.description"
    };

    public static string GetDefaultOrder() => "p.created_at DESC";
}