namespace Sky.Template.Backend.Infrastructure.Entities.Reports;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

public class ProductSalesReportEntity
{
    [DbManager.mColumn("product_id")] public Guid ProductId { get; set; }
    [DbManager.mColumn("product_name")] public string ProductName { get; set; } = string.Empty;
    [DbManager.mColumn("total_amount")] public decimal TotalAmount { get; set; }
    [DbManager.mColumn("sale_count")] public int OrderCount { get; set; }
}
