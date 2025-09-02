namespace Sky.Template.Backend.Contract.Requests.Suppliers;

public class CreateSupplierRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string SupplierType { get; set; } = "REGULAR";
    public string Status { get; set; } = "ACTIVE";
    public string? PaymentTerms { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? Notes { get; set; }
} 