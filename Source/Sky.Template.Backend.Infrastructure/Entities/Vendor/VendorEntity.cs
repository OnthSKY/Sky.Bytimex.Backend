using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Vendor;

[TableName("vendors")]
public class VendorEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("name")]
    public string Name { get; set; } = string.Empty;

    [DbManager.mColumn("slug")]
    public string Slug { get; set; } = string.Empty;

    [DbManager.mColumn("short_description")]
    public string? ShortDescription { get; set; }

    [DbManager.mColumn("logo_url")]
    public string? LogoUrl { get; set; }

    [DbManager.mColumn("banner_url")]
    public string? BannerUrl { get; set; }

    [DbManager.mColumn("email")]
    public string? Email { get; set; }

    [DbManager.mColumn("phone")]
    public string? Phone { get; set; }

    [DbManager.mColumn("address")]
    public string? Address { get; set; }

    [DbManager.mColumn("rating_avg")]
    public decimal RatingAvg { get; set; }

    [DbManager.mColumn("rating_count")]
    public int RatingCount { get; set; }

    [DbManager.mColumn("status")]
    public string Status { get; set; } = "ACTIVE";

    [DbManager.mColumn("verification_status")]
    public string VerificationStatus { get; set; } = Sky.Template.Backend.Core.Enums.VerificationStatus.PENDING.ToString();

    [DbManager.mColumn("verification_note")]
    public string? VerificationNote { get; set; }

    [DbManager.mColumn("verified_at")]
    public DateTime? VerifiedAt { get; set; }

    [DbManager.mColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [DbManager.mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [DbManager.mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    [DbManager.mColumn("delete_reason")]
    public string? DeleteReason { get; set; }
}
