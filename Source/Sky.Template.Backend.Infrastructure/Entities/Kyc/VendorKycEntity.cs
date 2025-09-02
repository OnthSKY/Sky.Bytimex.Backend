using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Kyc;

[TableName("vendor_kyc_submissions")]
public class VendorKycEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("vendor_id")] public Guid VendorId { get; set; }
    [DbManager.mColumn("legal_name")] public string LegalName { get; set; } = string.Empty;
    [DbManager.mColumn("tax_id")] public string TaxId { get; set; } = string.Empty;
    [DbManager.mColumn("documents")] public string? Documents { get; set; }
    [DbManager.mColumn("status")] public string Status { get; set; } = "PENDING";
    [DbManager.mColumn("rejection_reason")] public string? RejectionReason { get; set; }
    public bool IsDeleted { get; set; }
    [DbManager.mColumn("deleted_at")] public DateTime? DeletedAt { get; set; }
    [DbManager.mColumn("deleted_by")] public Guid? DeletedBy { get; set; }
    public string? DeleteReason { get; set; }
}
