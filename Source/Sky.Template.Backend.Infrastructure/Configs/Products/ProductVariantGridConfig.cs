using System;
using System.Collections.Generic;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.Products;

public static class ProductVariantGridConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "sku",           new ColumnMapping("v.sku",            typeof(string)) },
        { "isActive",      new ColumnMapping("v.is_active",      typeof(bool)) },
        { "price",         new ColumnMapping("v.price",          typeof(decimal)) },
        { "stockQuantity", new ColumnMapping("v.stock_quantity", typeof(int)) },
        { "createdAt",     new ColumnMapping("v.created_at",     typeof(DateTime)) }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "sku"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "v.sku"
    };

    public static string GetDefaultOrder() => "v.created_at DESC";
}