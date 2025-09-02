using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.OrderStatusLogs;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/order-status-logs")]
[ApiVersion("1.0")]
public class OrderStatusLogController : AdminBaseController
{
    private readonly IAdminOrderStatusLogService _orderStatusLogService;

    public OrderStatusLogController(IAdminOrderStatusLogService orderStatusLogService)
    {
        _orderStatusLogService = orderStatusLogService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> Get([FromQuery] Guid orderId)
        => await HandleServiceResponseAsync(() => _orderStatusLogService.GetByOrderIdAsync(orderId));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> Create([FromBody] CreateOrderStatusLogRequest request)
        => await HandleServiceResponseAsync(() => _orderStatusLogService.CreateAsync(request));
}

