using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Core.Extensions;

public static class HttpContextExtensions
{
    public static Guid GetUserId(this HttpContext context)
    {
        var userId = context.User.GetUserId();
        if (userId.HasValue)
            return userId.Value;

        throw new UnAuthorizedException(SharedResourceKeys.ClaimsUserIdNotFound); // 🗝️ Localization key
    }

    public static string GetUserEmail(this HttpContext context)
    {
        var email = context.User.GetEmail();
        return email ?? throw new UnAuthorizedException(SharedResourceKeys.ClaimsEmailNotFound); // 🗝️
    }

    public static string GetSchemaName(this HttpContext context)
    {
        var schema = context.User.GetClaim(ClaimTypes.GroupSid);
        return schema ?? throw new UnAuthorizedException(SharedResourceKeys.ClaimsSchemaNotFound); // 🗝️
    }

    public static (string email, Guid userId, string schemaName) GetCurrentUserInfo(this HttpContext context)
    {
        var userId = context.GetUserId();
        var email = context.GetUserEmail();
        var schema = context.GetSchemaName();
        return (email, userId, schema);
    }

    public static Guid? GetImpersonatedBy(this HttpContext context)
    {
        return context.User.GetImpersonatedBy();
    }

    public static (Guid? adminId, Guid userId, string email, string schemaName) GetCurrentUserAndAdminInfo(this HttpContext context)
    {
        var (email, userId, schemaName) = context.GetCurrentUserInfo();
        var adminId = context.GetImpersonatedBy();
        return (adminId, userId, email, schemaName);
    }
}