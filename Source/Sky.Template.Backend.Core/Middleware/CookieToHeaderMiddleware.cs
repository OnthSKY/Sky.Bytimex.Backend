using Microsoft.AspNetCore.Http;

namespace Sky.Template.Backend.Core.Middleware;

public class CookieToHeaderMiddleware
{
    private readonly RequestDelegate _next;

    public CookieToHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue("access_token", out var jwtToken))
        {
            if (!string.IsNullOrEmpty(jwtToken))
            {
                context.Request.Headers["Authorization"] = $"Bearer {jwtToken}";
            }
        }

        await _next(context);
    }
}
