using System.Collections.Generic;

namespace Sky.Template.Backend.Contract.Responses.Dashboard.Vendor;

public class VendorMonthlyStatsResponse
{
    public List<MonthlyOrderStatsDto> Stats { get; set; } = new();
}
