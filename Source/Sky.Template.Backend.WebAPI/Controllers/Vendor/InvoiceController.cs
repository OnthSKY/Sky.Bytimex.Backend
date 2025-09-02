using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Contract.Requests.Invoices;

namespace Sky.Template.Backend.WebAPI.Controllers.Vendor;

[ApiController]
[Route("api/vendor/invoices")]
[ApiVersion("1.0")]
public class InvoiceController : VendorBaseController
{
    private readonly IVendorInvoiceService _invoiceService;

    public InvoiceController(IVendorInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetInvoices([FromQuery] InvoiceFilterRequest request)
        => await HandleServiceResponseAsync(() => _invoiceService.GetFilteredPaginatedAsync(request));

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetInvoice(Guid id)
        => await HandleServiceResponseAsync(() => _invoiceService.GetByIdAsync(id));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest request)
        => await HandleServiceResponseAsync(() => _invoiceService.CreateAsync(request));

    [HttpPut("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> UpdateInvoice(Guid id, [FromBody] UpdateInvoiceRequest request)
        => await HandleServiceResponseAsync(() => _invoiceService.UpdateAsync(id, request));

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> DeleteInvoice(Guid id)
        => await HandleServiceResponseAsync(() => _invoiceService.DeleteAsync(id));
}

