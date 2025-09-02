 

using System.Security.Claims;
using Sky.Template.Backend.Core.Constants;

namespace Sky.Template.Backend.Core.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static IEnumerable<string> GetRoles(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal?.FindAll(ClaimTypes.Role)?.Select(c => c.Value) ?? Enumerable.Empty<string>();
    }

    public static IEnumerable<string> GetPermissions(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal?.FindAll(CustomClaimTypes.Permission)?.Select(c => c.Value) ?? Enumerable.Empty<string>();
    }

    public static string? GetClaim(this ClaimsPrincipal claimsPrincipal, string claimType)
    {
        return claimsPrincipal?.FindFirst(claimType)?.Value;
    }

    public static IEnumerable<string> GetClaims(this ClaimsPrincipal claimsPrincipal, string claimType)
    {
        return claimsPrincipal?.FindAll(claimType)?.Select(c => c.Value) ?? Enumerable.Empty<string>();
    }

    public static Guid? GetImpersonatedBy(this ClaimsPrincipal claimsPrincipal)
    {
        var value = claimsPrincipal?.FindFirst(CustomClaimTypes.ImpersonatedBy)?.Value;
        return Guid.TryParse(value, out var result) ? result : null;
    }

    public static Guid? GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var value = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out var result) ? result : null;
    }

    public static string? GetEmail(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal?.FindFirst(ClaimTypes.Email)?.Value;
    }

    public static string? GetUserName(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal?.FindFirst(ClaimTypes.Name)?.Value;
    }
}
