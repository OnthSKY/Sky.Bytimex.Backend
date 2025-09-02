
namespace Sky.Template.Backend.Contract.Responses.Dashboard.Vendor;

public class MonthlyOrderStatsDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}
