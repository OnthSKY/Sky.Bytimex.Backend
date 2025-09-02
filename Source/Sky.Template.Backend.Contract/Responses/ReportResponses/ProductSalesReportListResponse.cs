namespace Sky.Template.Backend.Contract.Responses.ReportResponses;

public class ProductSalesReportListResponse
{
    public List<ProductSalesReportDto> Items { get; set; } = new();
    public int TotalCount => Items.Count;
}
