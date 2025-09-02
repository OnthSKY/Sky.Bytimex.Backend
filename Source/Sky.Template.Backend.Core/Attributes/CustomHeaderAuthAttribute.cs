using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sky.Template.Backend.Core.Configs;
using Sky.Template.Backend.Core.Encryption;
using Sky.Template.Backend.Core.Enums;

namespace Sky.Template.Backend.Core.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class CustomHeaderAuthAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _headerName;
    private readonly string _expectedValue;

    public CustomHeaderAuthAttribute(string headerName = "SankoClientXFlowAuthToken", string expectedValue = "$anfloW-$kY-aD;")
    {
        _headerName = headerName;
        _expectedValue = expectedValue;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var encryptionService = context.HttpContext.RequestServices.GetRequiredService<IEncryptionService>();
        var encryptionConfig = context.HttpContext.RequestServices.GetRequiredService<IOptions<EncryptionConfig>>().Value;

        if (!context.HttpContext.Request.Headers.TryGetValue(_headerName, out var encryptedHeader))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        try
        {
            var decryptedValue = encryptionService.Decrypt(encryptedHeader, EncryptionKeyType.Mobile); // Todo geniþletilebilir
            if (!decryptedValue.StartsWith(_expectedValue))
            {
                context.Result = new UnauthorizedResult();
            }
        }
        catch
        {
            context.Result = new UnauthorizedResult();
        }
    }
}