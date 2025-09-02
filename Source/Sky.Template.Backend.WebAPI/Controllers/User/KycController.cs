using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Kyc;
using Sky.Template.Backend.WebAPI.Controllers.User;
using Sky.Template.Backend.Core.Extensions;

namespace Sky.Template.Backend.WebAPI.Controllers.User;

[ApiController]
[Route("api/user/kyc")]
[ApiVersion("1.0")]
public class KycController : UserBaseController
{
    private readonly IUserKycService _kycService;

    public KycController(IUserKycService kycService)
    {
        _kycService = kycService;
    }

    [HttpPost("start/v{version:apiVersion}")]
    public async Task<IActionResult> Start([FromBody] KycVerificationRequest request)
        => await HandleServiceResponseAsync(() => _kycService.StartVerificationAsync(request));

    [HttpGet("status/v{version:apiVersion}")]
    public async Task<IActionResult> Status()
        => await HandleServiceResponseAsync(() => _kycService.GetStatusAsync(HttpContext.GetUserId()));

    [HttpPost("resubmit")]
    public async Task<IActionResult> ResubmitKyc([FromForm] KycSubmissionRequest request)
    {
        await _kycService.ResubmitKycAsync(HttpContext.GetUserId(), request);
        return NoContent();
    }

    [HttpDelete("{kycId}")]
    public async Task<IActionResult> DeleteKyc(Guid kycId)
    {
        await _kycService.DeleteKycAsync(HttpContext.GetUserId(), kycId);
        return NoContent();
    }
}

