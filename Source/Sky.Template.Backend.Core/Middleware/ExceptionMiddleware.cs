using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Sky.Template.Backend.Core.Enums;
using Sky.Template.Backend.Core.Helpers;
using Sky.Template.Backend.Core.Localization;
using Sky.Template.Backend.Core.Exceptions;
using System;
using System.Linq;

namespace Sky.Template.Backend.Core.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ExceptionMiddleware(RequestDelegate next, IStringLocalizer<SharedResource> localizer)
    {
        _next = next;
        _localizer = localizer;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception e)
        {
            await HandleExceptionAsync(httpContext, e);
        }
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception e)
    {
        httpContext.Response.ContentType = "application/json";
        string uniqueTransactionId = Guid.NewGuid().ToString();

        var pathSegments = httpContext.Request.Path.Value?.Trim('/').Split('/') ?? Array.Empty<string>();
        var servicePath = pathSegments.Length > 2 ? string.Join("/", pathSegments.Skip(2)) : "UnknownService";

        var traceId = httpContext.TraceIdentifier;
        var userName = httpContext.User.Identity?.Name ?? "Anonymous";
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        string responseMessage = _localizer["UnexpectedError"];
        string logDetailMessage = string.Empty;
        string exceptionType = "General";

        switch (e)
        {
            case NotFoundException nfEx:
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                responseMessage = string.Format(_localizer[nfEx.ResourceKey], nfEx.FormatArgs);
                exceptionType = nameof(NotFoundException);
                break;


            case ValidationException validationException:
                httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                var localizedErrors = validationException.Errors
                    .Select(e => new ValidationFailure(e.PropertyName, _localizer[e.ErrorMessage]))
                    .ToList();
                await httpContext.Response.WriteAsync(new ValidationErrorDetails
                {
                    StatusCode = httpContext.Response.StatusCode,
                    Errors = localizedErrors,
                    Message = _localizer[validationException.Message],
                    TransactionId = uniqueTransactionId
                }.ToString());
                return;

            case AuthorizationException authorizationException:
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                responseMessage = _localizer[authorizationException.Message];
                exceptionType = nameof(BusinessRulesException);
                break;
            case ForbiddenException forbiddenException:
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                responseMessage = _localizer[forbiddenException.Message];
                exceptionType = nameof(BusinessRulesException);
                break;
            case BusinessRulesException businessException:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                responseMessage = businessException.Message;
                exceptionType = nameof(BusinessRulesException);
                break;

            case UnAuthorizedException userNotFoundException:
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                responseMessage = _localizer[userNotFoundException.Message];
                exceptionType = nameof(UnAuthorizedException);
                break;

            case DatabaseException dbException:
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                var defaultText = _localizer["DatabaseErrorDefault"];
                responseMessage = string.IsNullOrEmpty(dbException.Message)
                    ? defaultText
                    : $"{defaultText} Hata: {_localizer[dbException.Message]}";
                exceptionType = nameof(DatabaseException);
                logDetailMessage = $$"""
                [ERROR ID: {{uniqueTransactionId}}]
                Status: 500
                TraceId: {{traceId}}
                UserName: {{userName}}
                ClientIp: {{clientIp}}
                MessageKey: {{dbException.MessageKey}}
                Procedure: {{dbException.ProcedureName}}
                Query: {{dbException.Query}}
                Caller: {{dbException.CallerMethod}}
                Parameters: {{System.Text.Json.JsonSerializer.Serialize(dbException.Parameters)}}
                ErrorCode: {{dbException.ErrorCode}}
                ExceptionMessage: {{dbException.InnerException?.Message}}
                ExceptionStackTrace: {{dbException.InnerException?.StackTrace}}
            """;
                break;

            default:
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                responseMessage = _localizer["InternalServerError"];
                exceptionType = "SystemException";
                break;
        }

        if (string.IsNullOrEmpty(logDetailMessage))
        {
            logDetailMessage = $"""
            [ERROR ID: {uniqueTransactionId}]
            TraceId: {traceId}
            UserName: {userName}
            ClientIp: {clientIp}
            Exception: {e}
        """;
        }

        await LogHelper.WriteToFileAsync(
            message: logDetailMessage,
            logType: LogType.Error,
            exceptionType: exceptionType,
            serviceName: servicePath
        );

        try
        {
            var serviceType = Type.GetType("Sky.Template.Backend.Application.Services.Admin.IAdminErrorLogService, Sky.Template.Backend.Application");
            var requestType = Type.GetType("Sky.Template.Backend.Contract.Requests.ErrorLogs.CreateErrorLogRequest, Sky.Template.Backend.Contract");
            if (serviceType != null && requestType != null)
            {
                var service = ServiceTool.ServiceProvider.GetService(serviceType);
                if (service != null)
                {
                    var request = Activator.CreateInstance(requestType);
                    requestType.GetProperty("Message")?.SetValue(request, e.Message);
                    requestType.GetProperty("StackTrace")?.SetValue(request, e.StackTrace);
                    requestType.GetProperty("Source")?.SetValue(request, e.Source);
                    requestType.GetProperty("Path")?.SetValue(request, httpContext.Request.Path.Value);
                    requestType.GetProperty("Method")?.SetValue(request, httpContext.Request.Method);
                    var methodInfo = serviceType.GetMethod("LogErrorAsync");
                    var task = methodInfo?.Invoke(service, new[] { request }) as Task;
                    if (task != null) await task;
                }
            }
        }
        catch { }

        await WriteErrorResponse(httpContext, uniqueTransactionId, httpContext.Response.StatusCode, responseMessage);
    }

    private async Task WriteErrorResponse(HttpContext context, string TransactionId, int statusCode, string message)
    {
        await context.Response.WriteAsync(new ErrorDetails
        {
            StatusCode = statusCode,
            TransactionId = TransactionId,
            Message = message
        }.ToString());
    }
}
