using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Suppliers;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/suppliers")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class SupplierController : AdminBaseController
{
    private readonly IAdminSupplierService _supplierService;

    public SupplierController(IAdminSupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet("v{version:apiVersion}")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetAllSuppliers()
        => await HandleServiceResponseAsync(() => _supplierService.GetAllSuppliersAsync());

    [HttpGet("v{version:apiVersion}")]
    [MapToApiVersion("2")]
    public async Task<IActionResult> GetFilteredPaginatedSuppliers([FromQuery] SupplierFilterRequest request)
        => await HandleServiceResponseAsync(() => _supplierService.GetFilteredPaginatedSuppliersAsync(request));

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetSupplierById(Guid id)
        => await HandleServiceResponseAsync(() => _supplierService.GetSupplierByIdAsync(id));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequest request)
        => await HandleServiceResponseAsync(() => _supplierService.CreateSupplierAsync(request));

    [HttpPut("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> UpdateSupplier(Guid id, [FromBody] UpdateSupplierRequest request)
        => await HandleServiceResponseAsync(() => _supplierService.UpdateSupplierAsync(request));

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> DeleteSupplier(Guid id)
        => await HandleServiceResponseAsync(() => _supplierService.DeleteSupplierAsync(id));

    [HttpDelete("v{version:apiVersion}/{id}/soft")]
    public async Task<IActionResult> SoftDeleteSupplier(Guid id, [FromQuery] string? reason = "")
        => await HandleServiceResponseAsync(() => _supplierService.SoftDeleteSupplierAsync(id, reason));

    [HttpGet("v{version:apiVersion}/by-email/{email}")]
    public async Task<IActionResult> GetSupplierByEmail(string email)
        => await HandleServiceResponseAsync(() => _supplierService.GetSupplierByEmailAsync(email));

    [HttpGet("v{version:apiVersion}/by-tax-number/{taxNumber}")]
    public async Task<IActionResult> GetSupplierByTaxNumber(string taxNumber)
        => await HandleServiceResponseAsync(() => _supplierService.GetSupplierByTaxNumberAsync(taxNumber));
}
