using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Vendors;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/vendors")]
[ApiVersion("1.0")]
public class VendorController : AdminBaseController
{
    private readonly IAdminVendorService _vendorService;

    public VendorController(IAdminVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetFilteredPaginatedVendors([FromQuery] VendorFilterRequest request)
        => await HandleServiceResponseAsync(() => _vendorService.GetFilteredPaginatedVendorsAsync(request));

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetVendorById(Guid id)
        => await HandleServiceResponseAsync(() => _vendorService.GetVendorByIdAsync(id));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> CreateVendor([FromBody] CreateVendorRequest request)
        => await HandleServiceResponseAsync(() => _vendorService.CreateVendorAsync(request));

    [HttpPut("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> UpdateVendor(Guid id, [FromBody] UpdateVendorRequest request)
    {
        request.Id = id;
        return await HandleServiceResponseAsync(() => _vendorService.UpdateVendorAsync(request));
    }

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> DeleteVendor(Guid id)
        => await HandleServiceResponseAsync(() => _vendorService.DeleteVendorAsync(id));

    [HttpDelete("v{version:apiVersion}/{id}/soft")]
    public async Task<IActionResult> SoftDeleteVendor(Guid id, [FromQuery] string? reason = "")
        => await HandleServiceResponseAsync(() => _vendorService.SoftDeleteVendorAsync(id, reason ?? ""));

    [HttpGet("v{version:apiVersion}/by-email/{email}")]
    public async Task<IActionResult> GetVendorByEmail(string email)
        => await HandleServiceResponseAsync(() => _vendorService.GetVendorByEmailAsync(email));

    [HttpPost("v{version:apiVersion}/{id}/approve")]
    public async Task<IActionResult> ApproveVendor(Guid id, [FromQuery] string? note = "")
        => await HandleServiceResponseAsync(() => _vendorService.ApproveVendorAsync(id, note));

    [HttpPost("v{version:apiVersion}/{id}/reject")]
    public async Task<IActionResult> RejectVendor(Guid id, [FromQuery] string? note = "")
        => await HandleServiceResponseAsync(() => _vendorService.RejectVendorAsync(id, note));
}

