using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;

namespace Sky.Template.Backend.Infrastructure.Entities.Supplier;

[TableName("suppliers")]
public class SupplierEntity : BaseEntity<Guid>, ISoftDeletable
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
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public string? DeleteReason { get; set; }
} 