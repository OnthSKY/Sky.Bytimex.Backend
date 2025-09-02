using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Contract.Requests.Vendors;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;

namespace Sky.Template.Backend.WebAPI.Controllers.Vendor;

[ApiController]
[Route("api/vendor/vendors")]
[ApiVersion("1.0")]
public class VendorSelfController : VendorBaseController
{
    private readonly IVendorSelfService _vendorService;

    public VendorSelfController(IVendorSelfService vendorService)
    {
        _vendorService = vendorService;
    }

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetVendorById(Guid id)
        => await HandleServiceResponseAsync(() => _vendorService.GetVendorByIdAsync(id));

    [HttpPut("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> UpdateVendor(Guid id, [FromBody] UpdateVendorRequest request)
    {
        request.Id = id;
        return await HandleServiceResponseAsync(() => _vendorService.UpdateVendorAsync(request));
    }

    [HttpGet("v{version:apiVersion}/verification-status")]
    public async Task<IActionResult> GetVerificationStatus()
        => await HandleServiceResponseAsync(() => _vendorService.GetVerificationStatusAsync());
}

