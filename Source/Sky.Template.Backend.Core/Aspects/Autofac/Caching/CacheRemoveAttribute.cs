using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Helpers;
using Sky.Template.Backend.Core.Utilities.Interceptors;
using System.Globalization;
using Sky.Template.Backend.Core.Exceptions;

namespace Sky.Template.Backend.Core.Aspects.Autofac.Caching;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class CacheRemoveAttribute : MethodInterceptionBaseAttribute
{
    private readonly string _cacheKeyPatternName;
    private ICacheService? _cacheService;

    public CacheRemoveAttribute(string cacheKeyPatternName)
    {
        _cacheKeyPatternName = cacheKeyPatternName;
    }

    public override void Intercept(IInvocation invocation)
    {
        _cacheService ??= ServiceTool.ServiceProvider.GetService<ICacheService>()
                        ?? throw new BusinessRulesException("CacheServiceNotResolved");

        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var pattern = ResolvePattern(_cacheKeyPatternName, culture);

        var returnType = invocation.Method.ReturnType;
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var genericArgument = returnType.GetGenericArguments()[0];
            var method = typeof(CacheRemoveAttribute).GetMethod(nameof(HandleAsyncGeneric), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(genericArgument);
            invocation.ReturnValue = method.Invoke(this, new object[] { invocation, pattern });
        }
        else if (typeof(Task).IsAssignableFrom(returnType))
        {
            invocation.ReturnValue = HandleAsync(invocation, pattern);
        }
        else
        {
            invocation.Proceed();
            RemoveByPatternAsync(pattern).GetAwaiter().GetResult();
        }
    }

    private async Task HandleAsync(IInvocation invocation, string pattern)
    {
        invocation.Proceed();
        var task = (Task)invocation.ReturnValue;
        await task;
        await RemoveByPatternAsync(pattern);
    }

    private async Task<T> HandleAsyncGeneric<T>(IInvocation invocation, string pattern)
    {
        invocation.Proceed();
        var task = (Task<T>)invocation.ReturnValue;
        var result = await task;
        await RemoveByPatternAsync(pattern);
        return result;
    }

    private async Task RemoveByPatternAsync(string pattern)
    {
        var keys = await _cacheService!.SearchKeysAsync(pattern);
        foreach (var key in keys)
            await _cacheService.RemoveAsync(key);
    }

    private string ResolvePattern(string patternName, string culture)
    {
        var cacheKeysType = typeof(CacheKeys);

        var field = cacheKeysType.GetField(patternName, BindingFlags.Public | BindingFlags.Static);
        if (field != null)
        {
            return (string)field.GetValue(null)!;
        }

        var method = cacheKeysType.GetMethod(patternName, new[] { typeof(string) });
        if (method != null)
        {
            return (string)method.Invoke(null, new object[] { culture })!;
        }

        throw new BusinessRulesException("CacheKeyPatternNotFound", patternName);
    }
}
