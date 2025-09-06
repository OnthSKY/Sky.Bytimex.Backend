using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Configs.Suppliers;

public static class SupplierGridFilterConfig
{
    public static Dictionary<string, ColumnMapping> GetColumnMappings() => new(StringComparer.OrdinalIgnoreCase)
    {
        { "name",           new ColumnMapping("s.name",            typeof(string)) },
        { "contactPerson",  new ColumnMapping("s.contact_person",   typeof(string)) },
        { "email",          new ColumnMapping("s.email",           typeof(string)) },
        { "phone",          new ColumnMapping("s.phone",           typeof(string)) },
        { "taxNumber",      new ColumnMapping("s.tax_number",      typeof(string)) },
        { "taxOffice",      new ColumnMapping("s.tax_office",      typeof(string)) },
        { "supplierType",   new ColumnMapping("s.supplier_type",   typeof(string)) },
        { "status",         new ColumnMapping("s.status",          typeof(string)) },
        { "minCreditLimit", new ColumnMapping("s.credit_limit >= @minCreditLimit", typeof(decimal)) },
        { "maxCreditLimit", new ColumnMapping("s.credit_limit <= @maxCreditLimit", typeof(decimal)) },
        { "createdAt",      new ColumnMapping("s.created_at",      typeof(DateTime)) },
        { "updatedAt",      new ColumnMapping("s.updated_at",      typeof(DateTime)) }
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