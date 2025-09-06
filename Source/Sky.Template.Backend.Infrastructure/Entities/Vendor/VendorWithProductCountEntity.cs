using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Vendor;

[TableName("vendors")]
public class VendorWithProductCountEntity : VendorEntity
{
    [DbManager.mColumn("product_count")]
    public int ProductCount { get; set; }
}
