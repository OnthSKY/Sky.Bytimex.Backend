using System;
using System.Collections.Generic;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.Roles;

public static class RoleGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "name",        new ColumnMapping("r.name",        typeof(string)) },
        { "status",      new ColumnMapping("r.status",      typeof(string)) },
        { "description", new ColumnMapping("r.description", typeof(string)) },
        { "createdAt",   new ColumnMapping("r.created_at",  typeof(DateTime)) }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "name",
        "description"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "r.name",
        "r.description",
        "r.status"
    };

    public static string GetDefaultOrder() => "r.created_at DESC";
}