namespace Sky.Template.Backend.Contract.Responses.ReportResponses;

public class VendorSalesReportListResponse
{
    public List<VendorSalesReportDto> Items { get; set; } = new();
    public int TotalCount => Items.Count;
}
