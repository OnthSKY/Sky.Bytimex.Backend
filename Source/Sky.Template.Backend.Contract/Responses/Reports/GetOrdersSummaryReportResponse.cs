namespace Sky.Template.Backend.Contract.Responses.Reports;

public class GetOrdersSummaryReportResponse
{
    public Guid VendorId { get; set; }
    public decimal TotalAmount { get; set; }
    public int OrderCount { get; set; }
}
