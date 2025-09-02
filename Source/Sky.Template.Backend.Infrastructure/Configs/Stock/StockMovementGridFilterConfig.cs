namespace Sky.Template.Backend.Infrastructure.Configs.Stock;

public static class StockMovementGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "productId", "sm.product_id" },
        { "supplierId", "sm.supplier_id" },
        { "movementType", "sm.movement_type" },
        { "startDate", "sm.movement_date >= @startDate" },
        { "endDate", "sm.movement_date <= @endDate" }
    };

    public static string GetDefaultOrder() => "sm.movement_date DESC";
}
