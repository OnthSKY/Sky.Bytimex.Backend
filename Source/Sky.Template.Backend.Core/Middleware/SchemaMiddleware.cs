
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Sky.Template.Backend.Core.Context;

namespace Sky.Template.Backend.Core.Middleware;
public class SchemaMiddleware
{
    private readonly RequestDelegate _next;

    public SchemaMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;
        if (user.Identity.IsAuthenticated)
        {
            var schemaClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GroupSid);
            if (schemaClaim != null)
            {
                GlobalSchema.Name = schemaClaim.Value;
            }
            else
            {
                GlobalSchema.Name = "system";
            }
        }

        await _next(context);
    }
}
