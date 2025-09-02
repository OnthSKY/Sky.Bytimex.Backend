namespace Sky.Template.Backend.Contract.Responses.ReportResponses;

public class ProductSalesReportDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int OrderCount { get; set; }
}
