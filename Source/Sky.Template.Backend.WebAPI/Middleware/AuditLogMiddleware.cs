using Sky.Template.Backend.Application.Services;
using Sky.Template.Backend.Contract.Requests.AuditLog;
using System.Security.Claims;
using System.Text;
using UAParser;

namespace Sky.Template.Backend.WebAPI.Middleware;

public class AuditLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAuditLogService _auditLogService;

    public AuditLogMiddleware(RequestDelegate next, IAuditLogService auditLogService)
    {
        _next = next;
        _auditLogService = auditLogService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestTime = DateTime.Now;

        var requestBody = await ReadRequestBody(context.Request);

        var originalBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);
        }
        finally
        {
            var responseBody = await ReadResponseBody(context.Response);
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalBodyStream);

            var controllerName = context.GetRouteValue("controller")?.ToString();
            var actionName = context.GetRouteValue("action")?.ToString();
            var pageUrl = $"/{controllerName}/{actionName}";
            var method = context.Request.Method;

            var excludedUrls = new HashSet<string>
            {
                "/Auth/LoginWithPassword",
                "/Auth/LoginWithEmailOnly"
            };

            if (method != HttpMethod.Get.Method && controllerName != null && actionName != null && !excludedUrls.Contains(pageUrl))
            {

                var userAgent = context.Request.Headers.UserAgent.ToString();
                var uaParser = Parser.GetDefault();
                var info = uaParser.Parse(userAgent);

                var deviceFamily = info.Device.Family;
                var deviceType = "Desktop";

                if (userAgent.Contains("Mobi") || userAgent.Contains("Android") || userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
                {
                    deviceType = "Mobile";
                }

                int? userId = null;
                if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
                {
                    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(userIdClaim, out var parsedUserId))
                    {
                        userId = parsedUserId;
                    }
                }

                var auditLogRecord = new AuditLogParameters
                {
                    ActivityId = Guid.NewGuid(),
                    UserId = userId ?? -1,
                    EventName = actionName,
                    PageUrl = pageUrl,
                    RequestTime = requestTime,
                    ResponseTime = DateTime.Now,
                    RequestUrl = context.Request.Path,
                    ModuleName = controllerName,
                    RequestBody = requestBody,
                    ResponseBody = responseBody,
                    Browser = info.Browser.ToString(),
                    Device = info.OS.ToString(),
                    DeviceFamily = deviceFamily,
                    DeviceType = deviceType
                };


                await _auditLogService.Execute(auditLogRecord);

            }
        }
    }

    private async Task<string> ReadRequestBody(HttpRequest request)
    {
        if (request.ContentLength == null || request.ContentLength == 0)
            return string.Empty;

        request.EnableBuffering(); // Birden fazla kez okuyabilmek i�in
        request.Body.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        request.Body.Seek(0, SeekOrigin.Begin); // Sonraki middleware'ler i�in ba�a sar

        return body;
    }

    private async Task<string> ReadResponseBody(HttpResponse response)
    {
        if (!response.Body.CanSeek)
            return string.Empty;

        response.Body.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(response.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        response.Body.Seek(0, SeekOrigin.Begin);

        return body;
    }

}
