using System.Text;
using RestSharp;
using Sky.Template.Backend.Application.IntegrationModels.Common;
using Sky.Template.Backend.Application.Validators.FluentValidation.RestIntegration;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;

namespace Sky.Template.Backend.Application.Services;

public interface IRestIntegrationService
{
    Task<RestResponse> ExecuteAsync(RestIntegrationRequest request);

}


public class RestIntegrationService : IRestIntegrationService
{
    [ValidationAspect(typeof(RestIntegrationRequestValidator))]
    public async Task<RestResponse> ExecuteAsync(RestIntegrationRequest request)
    {
        var client = new RestClient(new RestClientOptions(request.Url));
        var restRequest = new RestRequest("", request.Method);

        if (request.Headers != null)
        {
            foreach (var header in request.Headers)
            {
                restRequest.AddHeader(header.Key, header.Value);
            }
        }

        // Auth header ekle (kontrollü)
        AddAuthHeaderIfNeeded(restRequest, request);

        if (!string.IsNullOrWhiteSpace(request.ContentType))
        {
            restRequest.AddHeader("Content-Type", request.ContentType);
        }

        if (request.Body != null)
        {
            restRequest.AddJsonBody(request.Body);
        }

        return await client.ExecuteAsync(restRequest);
    }

    private void AddAuthHeaderIfNeeded(RestRequest restRequest, RestIntegrationRequest request)
    {
        var headers = request.Headers ?? new Dictionary<string, string>();

        if (!request.RequiresAuthentication || string.IsNullOrEmpty(request.AuthType))
            return;

        // Eðer Authorization zaten eklendiyse dokunma
        if (headers.Keys.Any(k => k.Equals("Authorization", StringComparison.OrdinalIgnoreCase)))
            return;

        var key = string.IsNullOrWhiteSpace(request.AuthHeaderKey) ? "Authorization" : request.AuthHeaderKey;

        switch (request.AuthType)
        {
            case "Bearer":
                var bearerToken = request.AuthToken ?? "";
                restRequest.AddHeader(key,
                    bearerToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? bearerToken
                        : $"Bearer {bearerToken}");
                break;

            case "Basic":
                var encoded = Convert.ToBase64String( Encoding.UTF8.GetBytes(request.AuthToken ?? ""));
                restRequest.AddHeader(key, $"Basic {encoded}");
                break;

            case "ApiKey":
            case "Custom":
                restRequest.AddHeader(key, request.AuthToken ?? "");
                break;

            case "None":
            default:
                // Header eklenmez
                break;
        }
    }

}
