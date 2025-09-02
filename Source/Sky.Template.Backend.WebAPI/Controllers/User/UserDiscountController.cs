using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Discounts;
using Sky.Template.Backend.Core.Extensions;

namespace Sky.Template.Backend.WebAPI.Controllers.User;

[ApiController]
[Route("api/user/discounts")]
[ApiVersion("1.0")]
public class UserDiscountController : UserBaseController
{
    private readonly IUserDiscountService _discountService;

    public UserDiscountController(IUserDiscountService discountService)
    {
        _discountService = discountService;
    }

    [HttpPost("v{version:apiVersion}/apply")]
    public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponRequest request)
    {
        var buyerId = HttpContext.GetUserId();
        return await HandleServiceResponseAsync(() => _discountService.ApplyCouponAsync(buyerId, request));
    }
}
