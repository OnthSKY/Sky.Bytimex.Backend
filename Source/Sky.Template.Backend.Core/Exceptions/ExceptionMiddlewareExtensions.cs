using Microsoft.AspNetCore.Builder;
using Sky.Template.Backend.Core.Middleware;

namespace Sky.Template.Backend.Core.Exceptions;

public static class ExceptionMiddlewareExtensions
{
    public static void ConfigureCustomExceptionMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
    }
}
