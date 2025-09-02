using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Contract.Requests.Orders;

namespace Sky.Template.Backend.WebAPI.Controllers.Vendor;

[ApiController]
[Route("api/vendor/orders")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class OrderController : VendorBaseController
{
    private readonly IVendorOrderService _orderService;

    public OrderController(IVendorOrderService orderService)
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

    [HttpPut("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> UpdateSale(Guid id, [FromBody] UpdateOrderRequest request)
        => await HandleServiceResponseAsync(() => _orderService.UpdateSaleAsync(id, request));

    [HttpDelete("v{version:apiVersion}/{id}/soft")]
    public async Task<IActionResult> SoftDeleteSale(Guid id)
        => await HandleServiceResponseAsync(() => _orderService.SoftDeleteSaleAsync(id));

    [HttpPost("v{version:apiVersion}/{id}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id)
        => await HandleServiceResponseAsync(() => _orderService.CancelOrderAsync(id));

    [HttpPost("v{version:apiVersion}/{id}/reorder")]
    public async Task<IActionResult> ReorderOrder(Guid id)
        => await HandleServiceResponseAsync(() => _orderService.ReorderAsync(id));
}

