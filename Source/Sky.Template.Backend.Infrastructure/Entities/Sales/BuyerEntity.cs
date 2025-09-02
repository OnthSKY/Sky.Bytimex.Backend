using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Sales;

[TableName("buyers")]
public class BuyerEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("buyer_type")]
    public string BuyerType { get; set; } = string.Empty;

    [DbManager.mColumn("name")]
    public string? Name { get; set; }

    [DbManager.mColumn("email")]
    public string? Email { get; set; }

    [DbManager.mColumn("phone")]
    public string? Phone { get; set; }

    [DbManager.mColumn("company_name")]
    public string? CompanyName { get; set; }

    [DbManager.mColumn("tax_number")]
    public string? TaxNumber { get; set; }

    [DbManager.mColumn("tax_office")]
    public string? TaxOffice { get; set; }

    [DbManager.mColumn("description")]
    public string? Description { get; set; }

    [DbManager.mColumn("linked_user_id")]
    public Guid? LinkedUserId { get; set; }

    [DbManager.mColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [DbManager.mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [DbManager.mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    [DbManager.mColumn("delete_reason")]
    public string? DeleteReason { get; set; }
} 