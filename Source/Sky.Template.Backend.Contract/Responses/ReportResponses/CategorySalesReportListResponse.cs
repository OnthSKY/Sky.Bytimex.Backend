namespace Sky.Template.Backend.Contract.Responses.ReportResponses;

public class CategorySalesReportListResponse
{
    public List<CategorySalesReportDto> Items { get; set; } = new();
    public int TotalCount => Items.Count;
}
