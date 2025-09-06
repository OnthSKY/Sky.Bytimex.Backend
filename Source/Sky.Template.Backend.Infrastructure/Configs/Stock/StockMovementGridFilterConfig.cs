using System;
using System.Collections.Generic;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.Stock;

public static class StockMovementGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "productId",    new ColumnMapping("sm.product_id",    typeof(Guid)) },
        { "supplierId",   new ColumnMapping("sm.supplier_id",   typeof(Guid)) },
        { "movementType", new ColumnMapping("sm.movement_type", typeof(string)) },
        { "startDate",    new ColumnMapping("sm.movement_date >= @startDate", typeof(DateTime)) },
        { "endDate",      new ColumnMapping("sm.movement_date <= @endDate",   typeof(DateTime)) }
    };

    public static string GetDefaultOrder() => "sm.movement_date DESC";
}