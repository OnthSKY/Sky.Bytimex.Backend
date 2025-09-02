namespace Sky.Template.Backend.Infrastructure.Entities.Reports;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

public class PeriodSalesReportEntity
{
    [DbManager.mColumn("period")] public string Period { get; set; } = string.Empty;
    [DbManager.mColumn("total_amount")] public decimal TotalAmount { get; set; }
    [DbManager.mColumn("sale_count")] public int OrderCount { get; set; }
}
