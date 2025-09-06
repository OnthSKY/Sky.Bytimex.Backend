using Sky.Template.Backend.Core.Attributes;

namespace Sky.Template.Backend.Infrastructure.Entities.Vendor;

[TableName("vendors")]
public class VendorWithProductCountEntity : VendorEntity
{
    [DbManager.mColumn("product_count")]
    public int ProductCount { get; set; }
}
