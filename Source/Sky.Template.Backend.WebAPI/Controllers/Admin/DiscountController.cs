using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Discounts;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/discounts")]
[ApiVersion("1.0")]
public class DiscountController : AdminBaseController
{
    private readonly IAdminDiscountService _discountService;

    public DiscountController(IAdminDiscountService discountService)
    {
        _discountService = discountService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetDiscounts([FromQuery] DiscountFilterRequest request)
        => await HandleServiceResponseAsync(() => _discountService.GetDiscountsAsync(request));

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetDiscountById(Guid id)
        => await HandleServiceResponseAsync(() => _discountService.GetDiscountByIdAsync(id));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> CreateDiscount([FromBody] CreateDiscountRequest request)
        => await HandleServiceResponseAsync(() => _discountService.CreateDiscountAsync(request));

    [HttpPut("v{version:apiVersion}")]
    public async Task<IActionResult> UpdateDiscount([FromBody] UpdateDiscountRequest request)
        => await HandleServiceResponseAsync(() => _discountService.UpdateDiscountAsync(request));

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> DeleteDiscount(Guid id)
        => await HandleServiceResponseAsync(() => _discountService.DeleteDiscountAsync(id));
}
