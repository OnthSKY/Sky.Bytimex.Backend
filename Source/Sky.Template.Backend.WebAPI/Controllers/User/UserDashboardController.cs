using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.User;

namespace Sky.Template.Backend.WebAPI.Controllers.User;

[ApiController]
[Route("api/user/dashboard")]
[ApiVersion("1.0")]
public class UserDashboardController : UserBaseController
{
    private readonly IUserDashboardService _dashboardService;

    public UserDashboardController(IUserDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetUserPurchaseSummary()
        => await HandleServiceResponseAsync(() => _dashboardService.GetUserPurchaseSummaryAsync());

    [HttpGet("recent-views")]
    public async Task<IActionResult> GetRecentlyViewedProducts()
        => await HandleServiceResponseAsync(() => _dashboardService.GetRecentlyViewedProductsAsync());

    [HttpGet("recommendations")]
    public async Task<IActionResult> GetRecommendedProducts()
        => await HandleServiceResponseAsync(() => _dashboardService.GetRecommendedProductsAsync());
}
