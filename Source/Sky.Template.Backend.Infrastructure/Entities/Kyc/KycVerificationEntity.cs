using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Kyc;

[TableName("kyc_verifications")]
public class KycVerificationEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("user_id")] public Guid UserId { get; set; }
    [DbManager.mColumn("national_id")] public string NationalId { get; set; } = string.Empty;
    [DbManager.mColumn("country")] public string Country { get; set; } = string.Empty;
    [DbManager.mColumn("document_type")] public string DocumentType { get; set; } = string.Empty;
    [DbManager.mColumn("document_number")] public string DocumentNumber { get; set; } = string.Empty;
    [DbManager.mColumn("document_expiry_date")] public DateTime? DocumentExpiryDate { get; set; }
    [DbManager.mColumn("selfie_url")] public string? SelfieUrl { get; set; }
    [DbManager.mColumn("document_front_url")] public string? DocumentFrontUrl { get; set; }
    [DbManager.mColumn("document_back_url")] public string? DocumentBackUrl { get; set; }
    [DbManager.mColumn("status")] public string Status { get; set; } = "PENDING";
    [DbManager.mColumn("reason")] public string? Reason { get; set; }
    [DbManager.mColumn("reviewed_by")] public Guid? ReviewedBy { get; set; }
    [DbManager.mColumn("reviewed_at")] public DateTime? ReviewedAt { get; set; }
    public bool IsDeleted { get; set; }
    [DbManager.mColumn("deleted_at")] public DateTime? DeletedAt { get; set; }
    [DbManager.mColumn("deleted_by")] public Guid? DeletedBy { get; set; }
    [DbManager.mColumn("delete_reason")] public string? DeleteReason { get; set; }
}
