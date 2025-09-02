using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Sales;

[TableName("discount_usages")]
public class DiscountUsageEntity : BaseEntity<Guid>
{
    [DbManager.mColumn("discount_id")]
    public Guid DiscountId { get; set; }

    [DbManager.mColumn("buyer_id")]
    public Guid? BuyerId { get; set; }

    [DbManager.mColumn("order_id")]
    public Guid? OrderId { get; set; }

    [DbManager.mColumn("used_at")]
    public DateTime UsedAt { get; set; }
}
