using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Carts;

namespace Sky.Template.Backend.WebAPI.Controllers.User;

[ApiController]
[Route("api/user/carts")]
[ApiVersion("1.0")]
public class CartController : UserBaseController
{
    private readonly IUserCartService _cartService;

    public CartController(IUserCartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetCarts([FromQuery] CartFilterRequest request)
        => await HandleServiceResponseAsync(() => _cartService.GetFilteredPaginatedAsync(request));

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetCart(Guid id)
        => await HandleServiceResponseAsync(() => _cartService.GetByIdAsync(id));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> CreateCart([FromBody] CreateCartRequest request)
        => await HandleServiceResponseAsync(() => _cartService.CreateAsync(request));

    [HttpPut("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> UpdateCart(Guid id, [FromBody] UpdateCartRequest request)
        => await HandleServiceResponseAsync(() => _cartService.UpdateAsync(id, request));

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> DeleteCart(Guid id)
        => await HandleServiceResponseAsync(() => _cartService.DeleteAsync(id));
}
