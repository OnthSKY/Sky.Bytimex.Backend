using System;

namespace Sky.Template.Backend.Contract.Responses.Dashboard.Vendor;

public class VendorSalesSummaryResponse
{
    public decimal TotalSales { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
}
