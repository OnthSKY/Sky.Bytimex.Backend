using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Vendor;

namespace Sky.Template.Backend.WebAPI.Controllers.Vendor;

[ApiController]
[Route("api/vendor/dashboard")]
[ApiVersion("1.0")]
public class VendorDashboardController : VendorBaseController
{
    private readonly IVendorDashboardService _dashboardService;

    public VendorDashboardController(IVendorDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetVendorSalesSummary()
        => await HandleServiceResponseAsync(() => _dashboardService.GetVendorSalesSummaryAsync());

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopSellingProducts()
        => await HandleServiceResponseAsync(() => _dashboardService.GetTopSellingProductsAsync());

    [HttpGet("monthly-stats")]
    public async Task<IActionResult> GetMonthlyOrderChartData()
        => await HandleServiceResponseAsync(() => _dashboardService.GetMonthlyOrderChartDataAsync());

    [HttpGet("pending-operations")]
    public async Task<IActionResult> GetPendingOperations()
        => await HandleServiceResponseAsync(() => _dashboardService.GetPendingOperationsAsync());
}
