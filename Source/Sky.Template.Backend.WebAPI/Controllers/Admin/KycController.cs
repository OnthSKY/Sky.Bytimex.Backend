using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Kyc;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/kyc")]
[ApiVersion("1.0")]
public class KycController : AdminBaseController
{
    private readonly IAdminKycService _kycService;

    public KycController(IAdminKycService kycService)
    {
        _kycService = kycService;
    }

    [HttpPost("approve/v{version:apiVersion}")]
    public async Task<IActionResult> Approve([FromBody] KycApprovalRequest request)
        => await HandleServiceResponseAsync(() => _kycService.ApproveVerificationAsync(request));

    [HttpGet("status/v{version:apiVersion}/{userId}")]
    public async Task<IActionResult> Status(Guid userId)
        => await HandleServiceResponseAsync(() => _kycService.GetStatusAsync(userId));
}


