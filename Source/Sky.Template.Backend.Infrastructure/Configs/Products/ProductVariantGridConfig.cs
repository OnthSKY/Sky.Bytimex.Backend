using System;
using System.Collections.Generic;

namespace Sky.Template.Backend.Infrastructure.Configs.Products;

public static class ProductVariantGridConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "sku", "v.sku" },
        { "isActive", "v.is_active" },
        { "price", "v.price" },
        { "stockQuantity", "v.stock_quantity" },
        { "createdAt", "v.created_at" }
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

