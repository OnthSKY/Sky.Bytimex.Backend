using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Shipments;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/shipments")]
[ApiVersion("1.0")]
public class ShipmentController : AdminBaseController
{
    private readonly IAdminShipmentService _shipmentService;

    public ShipmentController(IAdminShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetShipments([FromQuery] ShipmentFilterRequest request)
        => await HandleServiceResponseAsync(() => _shipmentService.GetFilteredPaginatedAsync(request));

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetShipment(Guid id)
        => await HandleServiceResponseAsync(() => _shipmentService.GetByIdAsync(id));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> CreateShipment([FromBody] CreateShipmentRequest request)
        => await HandleServiceResponseAsync(() => _shipmentService.CreateAsync(request));

    [HttpPut("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> UpdateShipment(Guid id, [FromBody] UpdateShipmentRequest request)
        => await HandleServiceResponseAsync(() => _shipmentService.UpdateAsync(id, request));

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> DeleteShipment(Guid id)
        => await HandleServiceResponseAsync(() => _shipmentService.DeleteAsync(id));
}
