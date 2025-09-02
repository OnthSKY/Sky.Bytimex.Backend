using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Orders;

namespace Sky.Template.Backend.WebAPI.Controllers.User;

[ApiController]
[Route("api/user/orders")]
[ApiVersion("1.0")]
public class OrderController : UserBaseController
{
    private readonly IUserOrderService _orderService;

    public OrderController(IUserOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetSaleById(Guid id)
        => await HandleServiceResponseAsync(() => _orderService.GetSaleByIdAsync(id));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> CreateSale([FromBody] CreateOrderRequest request)
        => await HandleServiceResponseAsync(() => _orderService.CreateSaleAsync(request));

    [HttpPost("v{version:apiVersion}/{id}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id)
        => await HandleServiceResponseAsync(() => _orderService.CancelOrderAsync(id));

    [HttpPost("v{version:apiVersion}/{id}/reorder")]
    public async Task<IActionResult> ReorderOrder(Guid id)
        => await HandleServiceResponseAsync(() => _orderService.ReorderAsync(id));
}

