using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Sky.Template.Backend.Application.Services;
using System.Security.Claims;
using Sky.Template.Backend.Contract.Requests.Auth;

namespace Sky.Template.Backend.WebAPI.Middleware;

public class ClaimsUpdateMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ISharedAuthService _authService;
    private readonly ITokenService _tokenService;

    public ClaimsUpdateMiddleware(RequestDelegate next, ISharedAuthService authService, ITokenService tokenService)
    {
        _next = next;
        _authService = authService;
        _tokenService = tokenService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated ?? false)
        {
            var userEmail = context.User.FindFirst(ClaimTypes.Email)?.Value;

            if (!string.IsNullOrEmpty(userEmail))
            {
                var authResponse = await _authService.AuthenticateByEmailAsync(new AuthWithoutPasswordRequest(){Username = userEmail});

                if (authResponse is { Data.User: not null })
                {
                    var newClaims = await _tokenService.GetTokenClaimsByUserIdForUser(authResponse.Data.User.Id);

                    var existingClaims = context.User.Claims.ToList();

                    var claimsAreDifferent = !newClaims.All(newClaim =>
                        existingClaims.Any(existingClaim =>
                            existingClaim.Type == newClaim.Type &&
                            existingClaim.Value == newClaim.Value));

                    if (claimsAreDifferent)
                    {
                        var identity = new ClaimsIdentity(newClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        context.User = principal;
                        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    }
                }
            }
        }

        await _next(context);
    }
}
