namespace Sky.Template.Backend.Infrastructure.Configs.Sales;

public static class BuyerGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "name", "b.name" },
        { "email", "b.email" },
        { "phone", "b.phone" },
        { "companyName", "b.company_name" },
        { "taxNumber", "b.tax_number" },
        { "taxOffice", "b.tax_office" },
        { "createdAt", "b.created_at" }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "name",
        "email",
        "phone",
        "companyName",
        "taxNumber",
        "taxOffice"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "b.name",
        "b.email",
        "b.phone",
        "b.company_name",
        "b.tax_number",
        "b.tax_office"
    };

    public static string GetDefaultOrder() => "b.created_at DESC";
} 