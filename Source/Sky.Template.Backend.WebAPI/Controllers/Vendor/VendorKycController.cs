using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Contract.Requests.Kyc;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Constants;

namespace Sky.Template.Backend.WebAPI.Controllers.Vendor;

[ApiController]
[Route("api/vendors/v{version:apiVersion}/kyc")]
[ApiVersion("1.0")]
[Authorize]
[HasPermission(Permissions.Kyc.Manage)]
public class VendorKycController : VendorBaseController
{
    private readonly IVendorKycService _kycService;

    public VendorKycController(IVendorKycService kycService)
    {
        _kycService = kycService;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] VendorKycRequest request)
        => await HandleServiceResponseAsync(() => _kycService.SubmitVendorKycAsync(request));

    [HttpGet("{vendorId:guid}")]
    public async Task<IActionResult> Get(Guid vendorId)
        => await HandleServiceResponseAsync(() => _kycService.GetVendorKycAsync(vendorId));

    [HttpPost("review")]
    public async Task<IActionResult> Review([FromBody] VendorKycReviewRequest request)
        => await HandleServiceResponseAsync(() => _kycService.ReviewVendorKycAsync(request));
}

