using System.Net;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Core.BaseResponse;
public static class ControllerResponseBuilder
{
    public static BaseControllerResponse<T> NoContent<T>(string messageKey = "OperationSuccessful", HttpStatusCode statusCode = HttpStatusCode.NoContent)
        => CreateWithData(default(T), statusCode, messageKey);
    public static BaseControllerResponse<T> Success<T>(T data, string messageKey = "OperationSuccessful", HttpStatusCode statusCode = HttpStatusCode.OK)
        => CreateWithData(data, statusCode, messageKey);

    public static BaseControllerResponse Success(string messageKey = "OperationSuccessful", HttpStatusCode statusCode = HttpStatusCode.OK)
        => Create(statusCode, messageKey);

    public static BaseControllerResponse<T> Failure<T>(string messageKey = "OperationFailed", HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        => CreateWithData<T>(default, statusCode, messageKey);

    public static BaseControllerResponse Failure(string messageKey = "OperationFailed", HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        => Create(statusCode, messageKey);

    private static BaseControllerResponse<T> CreateWithData<T>(T? data, HttpStatusCode statusCode, string messageKey)
        => new BaseControllerResponse<T>
        {
            StatusCode = statusCode,
            Message = GetLocalizedMessage(messageKey),
            Data = data
        };

    private static BaseControllerResponse Create(HttpStatusCode statusCode, string messageKey)
        => new BaseControllerResponse
        {
            StatusCode = statusCode,
            Message = GetLocalizedMessage(messageKey)
        };
    public static BaseControllerResponse<T> Failure<T>(string message, object errorMeta, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        return new BaseControllerResponse<T>
        {
            Message = message,
            StatusCode = statusCode,
            ErrorMeta = errorMeta
        };
    }
    private static string GetLocalizedMessage(string key)
        => LocalizationProvider.Localizer?[key] ?? key;
}
