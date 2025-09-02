using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.DiscountUsages;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/discount-usages")]
[ApiVersion("1.0")]
public class DiscountUsageController : AdminBaseController
{
    private readonly IAdminDiscountUsageService _discountUsageService;

    public DiscountUsageController(IAdminDiscountUsageService discountUsageService)
    {
        _discountUsageService = discountUsageService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetUsages([FromQuery] DiscountUsageFilterRequest request)
        => await HandleServiceResponseAsync(() => _discountUsageService.GetDiscountUsagesAsync(request));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> CreateUsage([FromBody] CreateDiscountUsageRequest request)
        => await HandleServiceResponseAsync(() => _discountUsageService.CreateDiscountUsageAsync(request));
}
