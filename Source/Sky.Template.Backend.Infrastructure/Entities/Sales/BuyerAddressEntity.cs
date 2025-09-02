using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Sales;

[TableName("buyer_addresses")]
public class BuyerAddressEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("buyer_id")]
    public Guid BuyerId { get; set; }

    [DbManager.mColumn("address_title")]
    public string AddressTitle { get; set; } = string.Empty;

    [DbManager.mColumn("full_address")]
    public string FullAddress { get; set; } = string.Empty;

    [DbManager.mColumn("city")]
    public string City { get; set; } = string.Empty;

    [DbManager.mColumn("postal_code")]
    public string PostalCode { get; set; } = string.Empty;

    [DbManager.mColumn("country")]
    public string Country { get; set; } = string.Empty;

    [DbManager.mColumn("is_default")]
    public bool IsDefault { get; set; }

    [DbManager.mColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [DbManager.mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [DbManager.mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    [DbManager.mColumn("delete_reason")]
    public string? DeleteReason { get; set; }
}
