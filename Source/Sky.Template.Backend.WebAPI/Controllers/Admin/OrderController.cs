using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Orders;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/orders")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class OrderController : AdminBaseController
{
    private readonly IAdminOrderService _orderService;

    public OrderController(IAdminOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("v{version:apiVersion}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetAllSales()
        => await HandleServiceResponseAsync(() => _orderService.GetAllSalesAsync());

    [HttpGet("v{version:apiVersion}")]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetFilteredPaginatedSales([FromQuery] OrderFilterRequest request)
        => await HandleServiceResponseAsync(() => _orderService.GetFilteredPaginatedAsync(request));

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetSaleById(Guid id)
        => await HandleServiceResponseAsync(() => _orderService.GetSaleByIdAsync(id));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> CreateSale([FromBody] CreateOrderRequest request)
        => await HandleServiceResponseAsync(() => _orderService.CreateSaleAsync(request));

    [HttpPut("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> UpdateSale(Guid id, [FromBody] UpdateOrderRequest request)
        => await HandleServiceResponseAsync(() => _orderService.UpdateSaleAsync(id, request));

    [HttpDelete("v{version:apiVersion}/{id}/soft")]
    public async Task<IActionResult> SoftDeleteSale(Guid id)
        => await HandleServiceResponseAsync(() => _orderService.SoftDeleteSaleAsync(id));

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> HardDeleteSale(Guid id)
        => await HandleServiceResponseAsync(() => _orderService.HardDeleteSaleAsync(id));
}

