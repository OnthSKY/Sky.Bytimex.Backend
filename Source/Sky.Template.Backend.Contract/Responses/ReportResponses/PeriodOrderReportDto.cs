namespace Sky.Template.Backend.Contract.Responses.ReportResponses;

public class PeriodOrderReportDto
{
    public string Period { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int OrderCount { get; set; }
}
