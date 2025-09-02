using System;
using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Kyc;

[TableName("vendor_kyc_verifications")]
public class VendorKycVerificationEntity : BaseEntity<long>, ISoftDeletable
{
    [DbManager.mColumn("vendor_id")] public long VendorId { get; set; }
    [DbManager.mColumn("status")] public string Status { get; set; } = "PENDING";
    [DbManager.mColumn("notes")] public string? Notes { get; set; }
    [DbManager.mColumn("reviewed_by")] public long? ReviewedBy { get; set; }
    [DbManager.mColumn("reviewed_at")] public DateTime? ReviewedAt { get; set; }
    public bool IsDeleted { get; set; }
    [DbManager.mColumn("deleted_at")] public DateTime? DeletedAt { get; set; }
    [DbManager.mColumn("deleted_by")] public Guid? DeletedBy { get; set; }
    [DbManager.mColumn("delete_reason")] public string? DeleteReason { get; set; }
}
