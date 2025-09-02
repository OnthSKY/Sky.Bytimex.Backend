using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.CartItems;

namespace Sky.Template.Backend.WebAPI.Controllers.User;

[ApiController]
[Route("api/user/cart-items")]
[ApiVersion("1.0")]
public class CartItemController : UserBaseController
{
    private readonly IUserCartItemService _cartItemService;

    public CartItemController(IUserCartItemService cartItemService)
    {
        _cartItemService = cartItemService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetCartItems([FromQuery] Guid cartId)
        => await HandleServiceResponseAsync(() => _cartItemService.GetByCartIdAsync(cartId));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> CreateCartItem([FromBody] CreateCartItemRequest request)
        => await HandleServiceResponseAsync(() => _cartItemService.CreateAsync(request));

    [HttpPut("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> UpdateCartItem(Guid id, [FromBody] UpdateCartItemRequest request)
        => await HandleServiceResponseAsync(() => _cartItemService.UpdateAsync(id, request));

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> DeleteCartItem(Guid id)
        => await HandleServiceResponseAsync(() => _cartItemService.DeleteAsync(id));
}
