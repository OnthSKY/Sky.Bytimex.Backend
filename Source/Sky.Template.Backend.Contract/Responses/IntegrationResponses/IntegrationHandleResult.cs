using System.Net;
using Newtonsoft.Json;

namespace Sky.Template.Backend.Contract.Responses.IntegrationResponses;

 
public class IntegrationResultValidationErrorResponse
{
    [JsonProperty("Message")]
    public string Message { get; set; }

    [JsonProperty("Status")]
    public int StatusCode { get; set; }
    [JsonProperty("Errors")]
    public List<ErrorDetail> Errors { get; set; }
}

public class ErrorDetail
{
    [JsonProperty("PropertyName")]
    public string PropertyName { get; set; }

    [JsonProperty("ErrorMessage")]
    public string ErrorMessage { get; set; }

    [JsonProperty("AttemptedValue")]
    public object AttemptedValue { get; set; }

    [JsonProperty("CustomState")]
    public object CustomState { get; set; }

    [JsonProperty("Severity")]
    public int Severity { get; set; }

    [JsonProperty("ErrorCode")]
    public string ErrorCode { get; set; }

    [JsonProperty("FormattedMessagePlaceholderValues")]
    public Dictionary<string, object> FormattedMessagePlaceholderValues { get; set; }
}
public class IntegrationResultResponse
{
    [JsonProperty("message")]
    public string Message { get; set; }
    [JsonProperty("errorId")]
    public string? ErrorId { get; set; }
    [JsonProperty("statusCode")]
    public HttpStatusCode StatusCode { get; set; }
}

