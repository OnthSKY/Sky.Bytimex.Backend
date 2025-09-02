using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Reports;

public class VendorSalesSummaryEntity
{
    [DbManager.mColumn("vendor_id")] public Guid VendorId { get; set; }
    [DbManager.mColumn("total_amount")] public decimal TotalAmount { get; set; }
    [DbManager.mColumn("sale_count")] public int OrderCount { get; set; }
}
