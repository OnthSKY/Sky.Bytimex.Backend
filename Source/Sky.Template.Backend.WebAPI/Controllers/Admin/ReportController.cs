using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Reports;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/reports")]
[ApiVersion("1.0")]
public class ReportController : AdminBaseController
{
    private readonly IAdminReportService _reportService;

    public ReportController(IAdminReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("v{version:apiVersion}/vendors/sales")]
    public async Task<IActionResult> GetVendorSales([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        => await HandleServiceResponseAsync(() => _reportService.GetSalesByVendorAsync(startDate, endDate));

    [HttpGet("v{version:apiVersion}/categories/sales")]
    public async Task<IActionResult> GetCategorySales([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        => await HandleServiceResponseAsync(() => _reportService.GetSalesByCategoryAsync(startDate, endDate));

    [HttpGet("v{version:apiVersion}/products/sales")]
    public async Task<IActionResult> GetProductSales([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        => await HandleServiceResponseAsync(() => _reportService.GetSalesByProductAsync(startDate, endDate));

    [HttpGet("v{version:apiVersion}/summary")]
    public async Task<IActionResult> GetSalesSummary([FromQuery] string period, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        => await HandleServiceResponseAsync(() => _reportService.GetSalesSummaryAsync(period, startDate, endDate));

    [HttpGet("v{version:apiVersion}/vendors/count")]
    public async Task<IActionResult> GetVendorCount()
        => await HandleServiceResponseAsync(() => _reportService.GetVendorCountAsync());

    [HttpGet("v{version:apiVersion}/sales-summary")]
    public async Task<IActionResult> GetVendorSummary([FromQuery] GetSalesSummaryReportRequest request)
        => await HandleServiceResponseAsync(() => _reportService.GetVendorSalesSummaryAsync(request));
}
