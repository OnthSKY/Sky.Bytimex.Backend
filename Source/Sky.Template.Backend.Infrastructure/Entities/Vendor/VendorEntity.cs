using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;

namespace Sky.Template.Backend.Infrastructure.Entities.Vendor;

[TableName("vendors")]
public class VendorEntity : BaseEntity<Guid>, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public string VerificationStatus { get; set; } = Sky.Template.Backend.Core.Enums.VerificationStatus.PENDING.ToString();
    public string? VerificationNote { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public string? DeleteReason { get; set; }
}
