using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Contract.Requests.OrderStatusLogs;

namespace Sky.Template.Backend.WebAPI.Controllers.Vendor;

[ApiController]
[Route("api/vendor/order-status-logs")]
[ApiVersion("1.0")]
public class OrderStatusLogController : VendorBaseController
{
    private readonly IVendorOrderStatusLogService _orderStatusLogService;

    public OrderStatusLogController(IVendorOrderStatusLogService orderStatusLogService)
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

