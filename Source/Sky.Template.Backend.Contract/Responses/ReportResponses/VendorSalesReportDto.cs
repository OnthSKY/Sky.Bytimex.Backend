namespace Sky.Template.Backend.Contract.Responses.ReportResponses;

public class VendorSalesReportDto
{
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int OrderCount { get; set; }
}
