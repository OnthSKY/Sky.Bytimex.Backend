namespace Sky.Template.Backend.Contract.Responses.ReportResponses;

public class PeriodSalesReportListResponse
{
    public List<PeriodOrderReportDto> Items { get; set; } = new();
    public int TotalCount => Items.Count;
}
