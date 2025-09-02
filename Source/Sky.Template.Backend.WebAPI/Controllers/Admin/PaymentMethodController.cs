using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.PaymentMethods;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/payment-methods")]
[ApiVersion("1.0")]
public class PaymentMethodController : AdminBaseController
{
    private readonly IAdminPaymentMethodService _service;

    public PaymentMethodController(IAdminPaymentMethodService service)
    {
        _service = service;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetPaymentMethods([FromQuery] PaymentMethodFilterRequest request)
        => await HandleServiceResponseAsync(() => _service.GetFilteredPaginatedAsync(request));

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetPaymentMethod(Guid id)
        => await HandleServiceResponseAsync(() => _service.GetByIdAsync(id));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> CreatePaymentMethod([FromBody] CreatePaymentMethodRequest request)
        => await HandleServiceResponseAsync(() => _service.CreateAsync(request));

    [HttpPut("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> UpdatePaymentMethod(Guid id, [FromBody] UpdatePaymentMethodRequest request)
        => await HandleServiceResponseAsync(() => _service.UpdateAsync(id, request));

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> DeletePaymentMethod(Guid id)
        => await HandleServiceResponseAsync(() => _service.DeleteAsync(id));

    [HttpPost("v{version:apiVersion}/{id}/toggle")]
    public async Task<IActionResult> ToggleActivation(Guid id)
        => await HandleServiceResponseAsync(() => _service.ToggleActivationAsync(id));
}
