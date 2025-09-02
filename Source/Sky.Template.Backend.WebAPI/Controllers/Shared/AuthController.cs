using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sky.Template.Backend.Application.Services;
using Sky.Template.Backend.Contract.Requests.Auth;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Configs;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Core.Localization;
using Sky.Template.Backend.WebAPI.Controllers.Base;

namespace Sky.Template.Backend.WebAPI.Controllers.Shared;

[ApiController]
public class AuthController : CustomBaseController
{
    private readonly ISharedAuthService _authService;
    private readonly TokenManagerConfig _tokenManagerConfig;

    public AuthController(ISharedAuthService authService, IOptions<TokenManagerConfig> tokenManagerConfig)
    {
        _authService = authService;
        _tokenManagerConfig = tokenManagerConfig.Value;
    }

    [HttpPost("api/auth/sign-in")]
    public async Task<IActionResult> LoginWithPassword([FromBody] AuthRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.AuthenticateWithPasswordAsync(request, ip);

        if (result.Data is not null)
        {
            Response.Cookies.Append("refresh_token", result.Data.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = result.Data.RefreshExpireDate
            });

            Response.Cookies.Append("access_token", result.Data.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = result.Data.TokenExpireDate
            });
        }
        return Ok(result);
    }

    [CustomHeaderAuth]
    [HttpPost("api/auth/sign-in/email")]
    public async Task<IActionResult> LoginWithEmailOnly([FromBody] AuthWithoutPasswordRequest request)
        => await HandleServiceResponseAsync(() => _authService.AuthenticateByEmailAsync(request));

    [HttpPost("api/auth/refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken))
            refreshToken = Request.Headers["x-refresh-token"];
        if (string.IsNullOrEmpty(refreshToken) && Request.HasFormContentType)
            refreshToken = Request.Form["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken))
            throw new UnAuthorizedException(SharedResourceKeys.UserNotFound);

        var result = await _authService.RefreshTokenAsync(refreshToken);

        if (result.Data is null)
            throw new UnAuthorizedException(SharedResourceKeys.RefreshTokenInvalid);

        var isWebClient = Request.Cookies["refresh_token"] != null;
        if (isWebClient)
        {
            Response.Cookies.Append("refresh_token", result.Data.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(_tokenManagerConfig.RefreshTokenExpireDays)
            });

            Response.Cookies.Append("access_token", result.Data.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = result.Data.TokenExpireDate
            });

            return Ok(ControllerResponseBuilder.Success());
        }

        Response.Headers.Add("x-access-token", result.Data.Token);
        Response.Headers.Add("x-refresh-token", result.Data.RefreshToken);
        return Ok(ControllerResponseBuilder.Success(result.Data));
    }

    [HttpPost("api/auth/sign-out")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken))
            refreshToken = Request.Headers["x-refresh-token"];
        if (string.IsNullOrEmpty(refreshToken) && Request.HasFormContentType)
            refreshToken = Request.Form["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken))
            throw new UnAuthorizedException(SharedResourceKeys.RefreshTokenNotFound);

        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LogoutAsync(refreshToken, clientIp);

        if (Request.Cookies.ContainsKey("refresh_token"))
        {
            Response.Cookies.Delete("refresh_token");
            Response.Cookies.Delete("access_token");
        }

        return Ok(result);
    }

    [HttpGet("api/auth/me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        return Ok();
    }
}
