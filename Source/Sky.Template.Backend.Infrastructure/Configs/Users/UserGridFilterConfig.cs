using System;
using System.Collections.Generic;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.Users;

public static class UserGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "firstName", new ColumnMapping("u.name",    typeof(string)) },
        { "lastName",  new ColumnMapping("u.surname", typeof(string)) },
        { "email",     new ColumnMapping("u.email",   typeof(string)) },
        { "roleName",  new ColumnMapping("r.name",    typeof(string)) }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "firstName",
        "lastName",
        "email"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "u.name",
        "u.surname",
        "u.email",
        "r.name"
    };

    public static string GetDefaultOrder() => "u.name ASC";
}