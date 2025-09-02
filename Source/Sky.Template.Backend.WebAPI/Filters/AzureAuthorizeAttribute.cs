using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Sky.Template.Backend.Core.Constants;

namespace Sky.Template.Backend.WebAPI.Filters;

public class AzureAuthorizeAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var remoteIp = context.HttpContext.Connection.RemoteIpAddress?.ToString();
        bool ipAllowed = AzureIpAddresses.AddressPrefixes.Any(cidr => IsInCidrRange(remoteIp, cidr));

        if (!context.HttpContext.Request.Headers.TryGetValue("x-signature", out var requestHeaderValue) && !IsHeaderValueValid(requestHeaderValue, "Template-8#bD4%Smmy-31Oct24-AD-SKY-Q3R9z$") && !ipAllowed)
        {
            context.Result = new UnauthorizedResult(); // 401 Unauthorized
            return;
        }

        base.OnActionExecuting(context);
    }

    private static bool IsInCidrRange(string ipAddress, string cidr)
    {
        var parts = cidr.Split('/');
        var ip = IPAddress.Parse(ipAddress);
        var cidrIp = IPAddress.Parse(parts[0]);
        var prefixLength = int.Parse(parts[1]);

        var ipBytes = ip.GetAddressBytes();
        var cidrBytes = cidrIp.GetAddressBytes();

        int bytesToCheck = prefixLength / 8;
        int bitsToCheck = prefixLength % 8;

        for (int i = 0; i < bytesToCheck; i++)
        {
            if (ipBytes[i] != cidrBytes[i])
                return false;
        }

        if (bitsToCheck > 0)
        {
            int mask = (byte)(255 << (8 - bitsToCheck));
            if ((ipBytes[bytesToCheck] & mask) != (cidrBytes[bytesToCheck] & mask))
                return false;
        }

        return true;
    }
    private bool IsHeaderValueValid(string headerValue, string expectedValue)
    {
        return !string.IsNullOrEmpty(headerValue) && headerValue == expectedValue;
    }
}
