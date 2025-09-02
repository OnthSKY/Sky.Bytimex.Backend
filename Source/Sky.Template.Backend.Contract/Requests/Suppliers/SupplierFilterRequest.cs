using Sky.Template.Backend.Core.Requests.Base;

namespace Sky.Template.Backend.Contract.Requests.Suppliers;

public class SupplierFilterRequest : GridRequest
{
    public string? Name { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? SupplierType { get; set; }
    public string? Status { get; set; }
    public decimal? MinCreditLimit { get; set; }
    public decimal? MaxCreditLimit { get; set; }
} 