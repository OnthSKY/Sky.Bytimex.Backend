using System.Net;
using System.Text.Json.Serialization;

namespace Sky.Template.Backend.Core.BaseResponse;

public class BaseControllerResponse
{
    [JsonIgnore]
    public HttpStatusCode StatusCode { get; set; }
    public string Message { get; set; }
    public object? ErrorMeta { get; set; }

}

public class BaseControllerResponse<T> : BaseControllerResponse
{
    public T? Data { get; set; }
}

 
