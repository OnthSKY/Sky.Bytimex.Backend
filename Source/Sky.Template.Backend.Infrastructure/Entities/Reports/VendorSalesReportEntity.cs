namespace Sky.Template.Backend.Infrastructure.Entities.Reports;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

public class VendorSalesReportEntity
{
    [DbManager.mColumn("vendor_id")] public Guid VendorId { get; set; }
    [DbManager.mColumn("vendor_name")] public string VendorName { get; set; } = string.Empty;
    [DbManager.mColumn("total_amount")] public decimal TotalAmount { get; set; }
    [DbManager.mColumn("sale_count")] public int OrderCount { get; set; }
}
