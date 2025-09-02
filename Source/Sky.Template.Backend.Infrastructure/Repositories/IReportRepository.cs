using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Infrastructure.Entities.Reports;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Queries;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IReportRepository
{
    Task<IEnumerable<VendorSalesReportEntity>> GetOrdersByVendorAsync(DateTime? startDate, DateTime? endDate);
    Task<IEnumerable<CategoryOrdersReportEntity>> GetOrdersByCategoryAsync(DateTime? startDate, DateTime? endDate);
    Task<IEnumerable<ProductSalesReportEntity>> GetOrdersByProductAsync(DateTime? startDate, DateTime? endDate);
    Task<IEnumerable<PeriodSalesReportEntity>> GetOrdersByPeriodAsync(string period, DateTime? startDate, DateTime? endDate);
    Task<int> GetVendorCountAsync();
    Task<IEnumerable<VendorSalesSummaryEntity>> GetVendorSalesSummaryAsync(Guid? vendorId, DateTime? startDate, DateTime? endDate, string? currency);
}

public class ReportRepository : IReportRepository
{
    public async Task<IEnumerable<VendorSalesReportEntity>> GetOrdersByVendorAsync(DateTime? startDate, DateTime? endDate)
    {
        var parameters = new Dictionary<string, object>
        {
            {"@startDate", startDate},
            {"@endDate", endDate}
        };
        return await DbManager.ReadAsync<VendorSalesReportEntity>(ReportQueries.OrdersByVendor, parameters, GlobalSchema.Name);
    }

    public async Task<IEnumerable<CategoryOrdersReportEntity>> GetOrdersByCategoryAsync(DateTime? startDate, DateTime? endDate)
    {
        var parameters = new Dictionary<string, object>
        {
            {"@startDate", startDate},
            {"@endDate", endDate}
        };
        return await DbManager.ReadAsync<CategoryOrdersReportEntity>(ReportQueries.OrdersByCategory, parameters, GlobalSchema.Name);
    }

    public async Task<IEnumerable<ProductSalesReportEntity>> GetOrdersByProductAsync(DateTime? startDate, DateTime? endDate)
    {
        var parameters = new Dictionary<string, object>
        {
            {"@startDate", startDate},
            {"@endDate", endDate}
        };
        return await DbManager.ReadAsync<ProductSalesReportEntity>(ReportQueries.OrdersByProduct, parameters, GlobalSchema.Name);
    }

    public async Task<IEnumerable<PeriodSalesReportEntity>> GetOrdersByPeriodAsync(string period, DateTime? startDate, DateTime? endDate)
    {
        var parameters = new Dictionary<string, object>
        {
            {"@startDate", startDate},
            {"@endDate", endDate}
        };
        var sql = ReportQueries.OrdersByPeriodTemplate.Replace("{period}", period);
        return await DbManager.ReadAsync<PeriodSalesReportEntity>(sql, parameters, GlobalSchema.Name);
    }

    public async Task<int> GetVendorCountAsync()
    {
        var result = await DbManager.ReadAsync<DataCountEntity>(ReportQueries.VendorCount, null, GlobalSchema.Name);
        return result.FirstOrDefault()?.Count ?? 0;
    }

    public async Task<IEnumerable<VendorSalesSummaryEntity>> GetVendorSalesSummaryAsync(Guid? vendorId, DateTime? startDate, DateTime? endDate, string? currency)
    {
        var parameters = new Dictionary<string, object>
        {
            {"@vendor_id", vendorId},
            {"@startDate", startDate},
            {"@endDate", endDate},
            {"@currency", currency}
        };
        return await DbManager.ReadAsync<VendorSalesSummaryEntity>(ReportQueries.VendorOrdersSummary, parameters, GlobalSchema.Name);
    }
}
