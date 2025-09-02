using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Core.Context;

namespace Sky.Template.Backend.Core.Middleware;

public class ImpersonationMiddleware
{
    private readonly RequestDelegate _next;

    public ImpersonationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var impersonatedByClaimText = context.User.FindFirst("ImpersonatedBy")?.Value;

            if (Guid.TryParse(impersonatedByClaimText, out var impersonatedUserId) && impersonatedUserId != GlobalImpersonationContext.GetSentinelValue())
            {
                GlobalImpersonationContext.AdminId = impersonatedUserId;
            }
            else
            {
                GlobalImpersonationContext.AdminId = GlobalImpersonationContext.GetSentinelValue();
            }
        }

        await _next(context);
    }
}
