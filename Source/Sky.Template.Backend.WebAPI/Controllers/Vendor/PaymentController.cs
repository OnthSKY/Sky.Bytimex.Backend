using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Contract.Requests.Payments;
using Sky.Template.Backend.WebAPI.Controllers.Base;

namespace Sky.Template.Backend.WebAPI.Controllers.Vendor;

[ApiController]
[Route("api/vendor/payments")]
[ApiVersion("1.0")]
public class PaymentController : VendorBaseController
{
    private readonly IVendorPaymentService _paymentService;

    public PaymentController(IVendorPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetPayments([FromQuery] PaymentFilterRequest request)
        => await HandleServiceResponseAsync(() => _paymentService.GetFilteredPaginatedAsync(request));

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetPayment(Guid id)
        => await HandleServiceResponseAsync(() => _paymentService.GetByIdAsync(id));

    [HttpGet("v{version:apiVersion}/order/{orderId}")]
    public async Task<IActionResult> GetByOrder(Guid orderId)
        => await HandleServiceResponseAsync(() => _paymentService.GetByOrderIdAsync(orderId));

    [HttpGet("v{version:apiVersion}/buyer/{buyerId}")]
    public async Task<IActionResult> GetByBuyer(Guid buyerId)
        => await HandleServiceResponseAsync(() => _paymentService.GetByBuyerIdAsync(buyerId));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        => await HandleServiceResponseAsync(() => _paymentService.CreateAsync(request));

    [HttpPut("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> UpdatePayment(Guid id, [FromBody] UpdatePaymentRequest request)
        => await HandleServiceResponseAsync(() => _paymentService.UpdateAsync(id, request));

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> DeletePayment(Guid id)
        => await HandleServiceResponseAsync(() => _paymentService.DeleteAsync(id));
}
