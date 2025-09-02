using System.Collections.Generic;

namespace Sky.Template.Backend.Contract.Responses.Dashboard.Vendor;

public class VendorTopProductsResponse
{
    public List<TopProductDto> Products { get; set; } = new();
}
