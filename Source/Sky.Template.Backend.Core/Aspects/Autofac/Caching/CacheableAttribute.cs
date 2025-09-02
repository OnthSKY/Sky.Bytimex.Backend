using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Helpers;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Core.Utilities.Interceptors;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Sky.Template.Backend.Core.Exceptions;
using System.Threading;

namespace Sky.Template.Backend.Core.Aspects.Autofac.Caching;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class CacheableAttribute : MethodInterception
{
    public string CacheKeyPrefix { get; set; } = string.Empty;
    public int ExpirationInMinutes { get; set; }

    private ICacheService? _cacheService;
    private static readonly AsyncLocal<bool> _isInCache = new();

    protected override void OnBefore(IInvocation invocation)
    {
        _cacheService ??= ServiceTool.ServiceProvider.GetService<ICacheService>()
                          ?? throw new BusinessRulesException("CacheServiceNotResolved");

        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        var cacheKeysType = typeof(CacheKeys);
        string prefix;

        var fieldInfo = cacheKeysType.GetField(CacheKeyPrefix, BindingFlags.Public | BindingFlags.Static);
        if (fieldInfo != null)
        {
            prefix = (string)fieldInfo.GetValue(null)!;
        }
        else
        {
            var methodInfo = cacheKeysType.GetMethod(CacheKeyPrefix, new[] { typeof(string) });
            if (methodInfo != null)
            {
                prefix = (string)methodInfo.Invoke(null, new object[] { culture })!;
            }
            else
            {
                throw new BusinessRulesException("CacheKeyPrefixNotFound", CacheKeyPrefix);
            }
        }

        var sb = new StringBuilder(prefix);
        foreach (var arg in invocation.Arguments)
        {
            sb.Append(':').Append(JsonSerializer.Serialize(arg));
        }

        var key = sb.ToString();
        var returnType = invocation.Method.ReturnType;

        if (typeof(Task).IsAssignableFrom(returnType) && returnType.IsGenericType)
        {
            var genericArgument = returnType.GetGenericArguments()[0];
            var method = typeof(CacheableAttribute).GetMethod(nameof(HandleAsync), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(genericArgument);

            // 🔥 invocation.ReturnValue'yu set et ve Proceed çağırma!
            invocation.ReturnValue = method.Invoke(this, new object[] { invocation, key });
        }
        else
        {
            // Eğer Task değilse proceed et
            invocation.Proceed();
        }
    }

    private async Task<T> HandleAsync<T>(IInvocation invocation, string key)
    {
        var hasFilter = invocation.Arguments.Any(arg =>
            arg is GridRequest grid &&
            (grid.Filters?.Any() == true || !string.IsNullOrWhiteSpace(grid.SearchValue)));

        if (hasFilter)
        {
            invocation.Proceed();
            var task = (Task<T>)invocation.ReturnValue;
            return await task;
        }

        if (_isInCache.Value)
        {
            invocation.Proceed();
            var bypassTask = (Task<T>)invocation.ReturnValue;
            return await bypassTask;
        }

        _isInCache.Value = true;
        try
        {
            return await _cacheService!.GetOrSetAsync(
                key,
                async () =>
                {
                    invocation.Proceed();
                    var task = (Task<T>)invocation.ReturnValue;
                    var data = await task;

                    var type = typeof(T);
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(BaseControllerResponse<>))
                    {
                        var statusCodeProp = type.GetProperty("StatusCode");
                        if (statusCodeProp != null)
                        {
                            var status = (HttpStatusCode)statusCodeProp.GetValue(data)!;
                            if (status == 0)
                                statusCodeProp.SetValue(data, HttpStatusCode.OK);
                        }
                    }

                    return data;
                },
                new CacheEntryOptions
                {
                    AbsoluteExpiration = TimeSpan.FromMinutes(ExpirationInMinutes)
                });
        }
        finally
        {
            _isInCache.Value = false;
        }
    }

}
