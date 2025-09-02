using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Auth;
using Sky.Template.Backend.Contract.Responses.Auth;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Configs;
using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.WebAPI.Controllers.Base;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
public class AdminAuthController : CustomBaseController
{
    private readonly IAdminAuthService _authService;
    private readonly AzureAdLoginConfig _azureAdLoginConfig;

    public AdminAuthController(IAdminAuthService authService, IOptions<AzureAdLoginConfig> azureAdLoginConfig)
    {
        _authService = authService;
        _azureAdLoginConfig = azureAdLoginConfig.Value;
    }

    [Authorize("Admin")]
    [HttpPost("api/auth/impersonations")]
    public async Task<IActionResult> Impersonate([FromBody] ImpersonateRequest request)
        => Ok(await _authService.ImpersonateAsync(request));

    [Authorize]
    [HttpGet("api/auth/impersonations/status")]
    public IActionResult GetImpersonationStatus()
    {
        var adminId = HttpContext.GetImpersonatedBy();
        var isImpersonating = GlobalImpersonationContext.IsImpersonating;
        return Ok(new { IsImpersonating = isImpersonating, AdminId = adminId, CurrentUserId = HttpContext.GetUserId(), CurrentUserEmail = HttpContext.GetUserEmail() });
    }

    [Authorize("Admin")]
    [HttpPost("api/auth/impersonations/revert")]
    public async Task<IActionResult> ReturnUser()
        => Ok(await _authService.ReturnUserAsync());

    [HttpGet("api/auth/sign-in/azure")]
    public IActionResult RedirectToAdLogin()
    {
        string link = $"{_azureAdLoginConfig.AuthorityLink}/{_azureAdLoginConfig.TenantId}/oauth2/v2.0/authorize?" +
                      $"client_id={_azureAdLoginConfig.ClientId}" +
                      $"&response_type=code" +
                      $"&redirect_uri={Uri.EscapeDataString(_azureAdLoginConfig.RedirectUrl)}" +
                      $"&scope=openid%20profile%20email%20{Uri.EscapeDataString(_azureAdLoginConfig.ScopeLink)}";
        return Ok(ControllerResponseBuilder.Success<AzureLinkResponse>(new AzureLinkResponse { Link = link }, "Successful"));
    }

    [Route("signin-oidc")]
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> LoginWithAD([FromQuery] string? code, [FromQuery] string? error, [FromQuery] string? errorDescription)
        => Ok(await _authService.LoginWithADAsync(code, error, errorDescription));
}
