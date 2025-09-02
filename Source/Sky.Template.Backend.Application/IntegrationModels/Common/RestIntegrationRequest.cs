using RestSharp;

namespace Sky.Template.Backend.Application.IntegrationModels.Common;
public class RestIntegrationRequest
{
    public string Url { get; set; } = null!;
    public Method Method { get; set; } // RestSharp.Method kullanılmalı

    public string? ContentType { get; set; } = "application/json";

    public object? Body { get; set; }

    public Dictionary<string, string>? Headers { get; set; }

    public bool RequiresAuthentication { get; set; } = false;

    /// <summary>
    /// Token veya Basic auth string (örn: "token" ya da "username:password")
    /// </summary>
    public string? AuthToken { get; set; }

    /// <summary>
    /// Bearer, Basic, ApiKey, Custom, None
    /// </summary>
    public string? AuthType { get; set; }

    /// <summary>
    /// Authorization header için kullanýlacak key, örn: "Authorization", "X-API-KEY"
    /// </summary>
    public string? AuthHeaderKey { get; set; }
}
