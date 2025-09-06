using System;
using System.Collections.Generic;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.ErrorLogs;

public static class ErrorLogGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "message",   new ColumnMapping("el.message",   typeof(string)) },
        { "source",    new ColumnMapping("el.source",    typeof(string)) },
        { "path",      new ColumnMapping("el.path",      typeof(string)) },
        { "method",    new ColumnMapping("el.method",    typeof(string)) },
        { "createdAt", new ColumnMapping("el.created_at",typeof(DateTime)) }
    };

    public static string GetDefaultOrder() => "el.created_at DESC";

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "message",
        "source",
        "path",
        "method"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "el.message",
        "el.source",
        "el.path",
        "el.method"
    };
}