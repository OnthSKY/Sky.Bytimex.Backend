namespace Sky.Template.Backend.Contract.Responses.ReportResponses;

public class CategorySalesReportDto
{
    public Guid? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int OrderCount { get; set; }
}
