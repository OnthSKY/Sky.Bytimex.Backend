using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.ReportResponses;
using Sky.Template.Backend.Contract.Requests.Reports;
using Sky.Template.Backend.Contract.Responses.Reports;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminReportService
{
    Task<BaseControllerResponse<VendorSalesReportListResponse>> GetSalesByVendorAsync(DateTime? startDate, DateTime? endDate);
    Task<BaseControllerResponse<CategorySalesReportListResponse>> GetSalesByCategoryAsync(DateTime? startDate, DateTime? endDate);
    Task<BaseControllerResponse<ProductSalesReportListResponse>> GetSalesByProductAsync(DateTime? startDate, DateTime? endDate);
    Task<BaseControllerResponse<PeriodSalesReportListResponse>> GetSalesSummaryAsync(string period, DateTime? startDate, DateTime? endDate);
    Task<BaseControllerResponse<VendorCountResponse>> GetVendorCountAsync();
    Task<BaseControllerResponse<IEnumerable<GetOrdersSummaryReportResponse>>> GetVendorSalesSummaryAsync(GetSalesSummaryReportRequest request);
}

public class AdminReportService : IAdminReportService
{
    private readonly IReportRepository _reportRepository;

    public AdminReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }
    [HasPermission(Permissions.AdminReports.View)]
    public async Task<BaseControllerResponse<VendorSalesReportListResponse>> GetSalesByVendorAsync(DateTime? startDate, DateTime? endDate)
    {
        var data = await _reportRepository.GetOrdersByVendorAsync(startDate, endDate);
        var response = new VendorSalesReportListResponse
        {
            Items = data.Select(d => new VendorSalesReportDto
            {
                VendorId = d.VendorId,
                VendorName = d.VendorName,
                TotalAmount = d.TotalAmount,
                OrderCount = d.OrderCount
            }).ToList()
        };
        return ControllerResponseBuilder.Success(response);
    }
    [HasPermission(Permissions.AdminReports.View)]
    public async Task<BaseControllerResponse<CategorySalesReportListResponse>> GetSalesByCategoryAsync(DateTime? startDate, DateTime? endDate)
    {
        var data = await _reportRepository.GetOrdersByCategoryAsync(startDate, endDate);
        var response = new CategorySalesReportListResponse
        {
            Items = data.Select(d => new CategorySalesReportDto
            {
                CategoryId = d.CategoryId,
                CategoryName = d.CategoryName,
                TotalAmount = d.TotalAmount,
                OrderCount = d.OrderCount
            }).ToList()
        };
        return ControllerResponseBuilder.Success(response);
    }

    [HasPermission(Permissions.AdminReports.View)]
    public async Task<BaseControllerResponse<ProductSalesReportListResponse>> GetSalesByProductAsync(DateTime? startDate, DateTime? endDate)
    {
        var data = await _reportRepository.GetOrdersByProductAsync(startDate, endDate);
        var response = new ProductSalesReportListResponse
        {
            Items = data.Select(d => new ProductSalesReportDto
            {
                ProductId = d.ProductId,
                ProductName = d.ProductName,
                TotalAmount = d.TotalAmount,
                OrderCount = d.OrderCount
            }).ToList()
        };
        return ControllerResponseBuilder.Success(response);
    }
    [HasPermission(Permissions.AdminReports.View)]
    public async Task<BaseControllerResponse<PeriodSalesReportListResponse>> GetSalesSummaryAsync(string period, DateTime? startDate, DateTime? endDate)
    {
        var data = await _reportRepository.GetOrdersByPeriodAsync(period, startDate, endDate);
        var response = new PeriodSalesReportListResponse
        {
            Items = data.Select(d => new PeriodOrderReportDto
            {
                Period = d.Period,
                TotalAmount = d.TotalAmount,
                OrderCount = d.OrderCount
            }).ToList()
        };
        return ControllerResponseBuilder.Success(response);
    }

    [HasPermission(Permissions.AdminReports.View)]
    public async Task<BaseControllerResponse<VendorCountResponse>> GetVendorCountAsync()
    {
        var count = await _reportRepository.GetVendorCountAsync();
        var response = new VendorCountResponse { Count = count };
        return ControllerResponseBuilder.Success(response);
    }

    [HasPermission(Permissions.AdminReports.View)]
    public async Task<BaseControllerResponse<IEnumerable<GetOrdersSummaryReportResponse>>> GetVendorSalesSummaryAsync(GetSalesSummaryReportRequest request)
    {
        var data = await _reportRepository.GetVendorSalesSummaryAsync(request.VendorId, request.StartDate, request.EndDate, request.Currency);
        var mapped = data.Select(d => new GetOrdersSummaryReportResponse
        {
            VendorId = d.VendorId,
            TotalAmount = d.TotalAmount,
            OrderCount = d.OrderCount
        });
        return ControllerResponseBuilder.Success(mapped);
    }
}
