using System;
using System.Collections.Generic;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.Sales;

public static class DiscountUsageGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "discountId", new ColumnMapping("du.discount_id", typeof(Guid)) },
        { "buyerId",    new ColumnMapping("du.buyer_id",    typeof(Guid)) },
        { "orderId",    new ColumnMapping("du.order_id",    typeof(Guid)) },
        { "createdAt",  new ColumnMapping("du.created_at",  typeof(DateTime)) }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase);

    public static List<string> GetSearchColumns() => new()
    {
        // UUID alanlar text'e cast edilerek search yapılabilir
        "du.discount_id::text",
        "du.buyer_id::text",
        "du.order_id::text"
    };

    public static string GetDefaultOrder() => "du.created_at DESC";
}