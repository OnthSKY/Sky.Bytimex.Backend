using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.User;

namespace Sky.Template.Backend.WebAPI.Controllers.User;

[ApiController]
[Route("api/user/shipments")]
[ApiVersion("1.0")]
public class ShipmentController : UserBaseController
{
    private readonly IUserShipmentService _shipmentService;

    public ShipmentController(IUserShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    [HttpGet("v{version:apiVersion}/by-order/{orderId}")]
    public async Task<IActionResult> GetShipmentByOrderId(Guid orderId)
        => await HandleServiceResponseAsync(() => _shipmentService.GetShipmentByOrderIdAsync(orderId));

    [HttpGet("v{version:apiVersion}/tracking/{trackingNumber}")]
    public async Task<IActionResult> TrackShipment(string trackingNumber)
        => await HandleServiceResponseAsync(() => _shipmentService.TrackShipmentAsync(trackingNumber));
}
