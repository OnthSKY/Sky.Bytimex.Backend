using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.StockMovements;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/stock-movements")]
[ApiVersion("1.0")]
public class StockMovementController : AdminBaseController
{
    private readonly IAdminStockMovementService _stockMovementService;

    public StockMovementController(IAdminStockMovementService stockMovementService)
    {
        _stockMovementService = stockMovementService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> Get([FromQuery] StockMovementFilterRequest request)
        => await HandleServiceResponseAsync(() => _stockMovementService.GetAsync(request));

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetById(Guid id)
        => await HandleServiceResponseAsync(() => _stockMovementService.GetByIdAsync(id));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> Create([FromBody] CreateStockMovementRequest request)
        => await HandleServiceResponseAsync(() => _stockMovementService.CreateAsync(request));

    [HttpPut("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStockMovementRequest request)
        => await HandleServiceResponseAsync(() => _stockMovementService.UpdateAsync(id, request));

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> Delete(Guid id)
        => await HandleServiceResponseAsync(() => _stockMovementService.DeleteAsync(id));
}
