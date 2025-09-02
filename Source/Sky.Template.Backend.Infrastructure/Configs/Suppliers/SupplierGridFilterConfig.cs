namespace Sky.Template.Backend.Infrastructure.Configs.Suppliers;

public static class SupplierGridFilterConfig
{
    public static Dictionary<string, string> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "name", "s.name" },
        { "contactPerson", "s.contact_person" },
        { "email", "s.email" },
        { "phone", "s.phone" },
        { "taxNumber", "s.tax_number" },
        { "taxOffice", "s.tax_office" },
        { "supplierType", "s.supplier_type" },
        { "status", "s.status" },
        { "minCreditLimit", "s.credit_limit >= @minCreditLimit" },
        { "maxCreditLimit", "s.credit_limit <= @maxCreditLimit" },
        { "createdAt", "s.created_at" },
        { "updatedAt", "s.updated_at" }
    };

    public static HashSet<string> GetLikeFilterKeys() => new(StringComparer.OrdinalIgnoreCase)
    {
        "name",
        "contactPerson",
        "email",
        "phone",
        "taxNumber",
        "taxOffice"
    };

    public static List<string> GetSearchColumns() => new()
    {
        "s.name",
        "s.contact_person",
        "s.email",
        "s.phone",
        "s.tax_number",
        "s.tax_office",
        "s.address",
        "s.notes"
    };

    public static string GetDefaultOrder() => "s.created_at DESC";
} 