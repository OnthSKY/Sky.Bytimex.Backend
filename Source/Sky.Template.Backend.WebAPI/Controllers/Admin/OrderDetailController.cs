using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.OrderDetails;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/order-details")]
[ApiVersion("1.0")]
public class OrderDetailController : AdminBaseController
{
    private readonly IAdminOrderDetailService _orderDetailService;

    public OrderDetailController(IAdminOrderDetailService orderDetailService)
    {
        _orderDetailService = orderDetailService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetByOrderId([FromQuery] Guid orderId)
        => await HandleServiceResponseAsync(() => _orderDetailService.GetByOrderIdAsync(orderId));

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetById(Guid id)
        => await HandleServiceResponseAsync(() => _orderDetailService.GetByIdAsync(id));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> Create([FromBody] CreateOrderDetailRequest request)
        => await HandleServiceResponseAsync(() => _orderDetailService.CreateAsync(request));

    [HttpPut("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderDetailRequest request)
        => await HandleServiceResponseAsync(() => _orderDetailService.UpdateAsync(id, request));

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> Delete(Guid id)
        => await HandleServiceResponseAsync(() => _orderDetailService.DeleteAsync(id));
}

