using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.User;

[TableName("users")]
public class UserEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("username")]
    public string Username { get; set; } = string.Empty;

    [DbManager.mColumn("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [DbManager.mColumn("last_name")]
    public string LastName { get; set; } = string.Empty;

    [DbManager.mColumn("email")]
    public string Email { get; set; } = string.Empty;

    [DbManager.mColumn("image_path")]
    public string? ImagePath { get; set; }

    [DbManager.mColumn("phone")]
    public string Phone { get; set; } = string.Empty;

    [DbManager.mColumn("vendor_id")]
    public Guid? VendorId { get; set; }

    [DbManager.mColumn("kyc_status")]
    public string? KycStatus { get; set; }

    [DbManager.mColumn("is_email_verified")]
    public bool IsEmailVerified { get; set; }

    [DbManager.mColumn("preferred_language")]
    public string? PreferredLanguage { get; set; }

    [DbManager.mColumn("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [DbManager.mColumn("status")]
    public string Status { get; set; } = "ACTIVE";

    [DbManager.mColumn("referred_by")]
    public Guid? ReferredBy { get; set; }

    [DbManager.mColumn("last_login_date")]
    public DateTime? LastLoginDate { get; set; }

    [DbManager.mColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [DbManager.mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [DbManager.mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    [DbManager.mColumn("delete_reason")]
    public string? DeleteReason { get; set; }
}
