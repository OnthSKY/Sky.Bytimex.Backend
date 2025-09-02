using Azure;
using Microsoft.Extensions.Options;
using Sky.Template.Backend.Application.Services;
using Sky.Template.Backend.Core.Configs;
using Sky.Template.Backend.Core.Localization;
using System.IdentityModel.Tokens.Jwt;

public class SilentAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ISharedAuthService _authService;
    private readonly TokenManagerConfig _tokenManagerConfig;

    public SilentAuthMiddleware(RequestDelegate next, ISharedAuthService authService, IOptions<TokenManagerConfig> tokenManagerConfig)
    {
        _next = next;
        _authService = authService;
        _tokenManagerConfig = tokenManagerConfig.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var accessToken = context.Request.Cookies["access_token"];
        var refreshToken = context.Request.Cookies["refresh_token"];

        var isAccessTokenExpired = IsTokenExpired(accessToken);


        if (isAccessTokenExpired && !string.IsNullOrEmpty(refreshToken))
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(refreshToken);

                if (result.Data is not null)
                {
                    var accessExpire = result.Data.TokenExpireDate;

                    var refreshExpire = DateTime.UtcNow.AddDays(_tokenManagerConfig.RefreshTokenExpireDays);

                    context.Response.Cookies.Append("access_token", result.Data.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = accessExpire
                    });

                    context.Response.Cookies.Append("refresh_token", result.Data.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = refreshExpire
                    });

                    // Access token'ı header'a koy (opsiyonel ama Authorization header’ı middleware’ler için lazımsa önemlidir)
                    context.Request.Headers["Authorization"] = $"Bearer {result.Data.Token}";
                }
            }
            catch (Exception e)
            {
                if (e.Message == SharedResourceKeys.RefreshTokenInvalid && context.Request.Cookies.ContainsKey("refresh_token"))
                {

                    context.Response.Cookies.Delete("refresh_token");
                    context.Response.Cookies.Delete("access_token");

                }
            }
        }
        await _next(context);

    }

    private bool IsTokenExpired(string? token)
    {
        if (string.IsNullOrEmpty(token)) return true;

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadToken(token) as JwtSecurityToken;
        return jwt == null || jwt.ValidTo < DateTime.UtcNow;
    }
}
