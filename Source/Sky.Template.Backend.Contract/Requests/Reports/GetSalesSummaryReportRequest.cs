namespace Sky.Template.Backend.Contract.Requests.Reports;

public class GetSalesSummaryReportRequest : BaseRequest
{
    public Guid? VendorId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Currency { get; set; }
}
