using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Helpers;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Utilities.Interceptors;

namespace Sky.Template.Backend.Core.Aspects.Autofac.Caching;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class InvalidateCacheAttribute : MethodInterceptionBaseAttribute
{
    private readonly string _cacheKeyPrefix;
    private ICacheService? _cacheService;

    public InvalidateCacheAttribute(string cacheKeyPrefix)
    {
        _cacheKeyPrefix = cacheKeyPrefix;
    }

    public override void Intercept(IInvocation invocation)
    {
        _cacheService ??= ServiceTool.ServiceProvider.GetService<ICacheService>()
            ?? throw new BusinessRulesException("CacheServiceNotResolved");

        var returnType = invocation.Method.ReturnType;
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var genericArgument = returnType.GetGenericArguments()[0];
            var method = typeof(InvalidateCacheAttribute).GetMethod(nameof(HandleAsyncGeneric), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(genericArgument);
            invocation.ReturnValue = method.Invoke(this, new object[] { invocation });
        }
        else if (typeof(Task).IsAssignableFrom(returnType))
        {
            invocation.ReturnValue = HandleAsync(invocation);
        }
        else
        {
            invocation.Proceed();
            RemoveWithSmartPatternAsync(invocation).GetAwaiter().GetResult();
        }
    }

    private async Task HandleAsync(IInvocation invocation)
    {
        invocation.Proceed();
        var task = (Task)invocation.ReturnValue;
        await task;
        await RemoveWithSmartPatternAsync(invocation);
    }

    private async Task<T> HandleAsyncGeneric<T>(IInvocation invocation)
    {
        invocation.Proceed();
        var task = (Task<T>)invocation.ReturnValue;
        var result = await task;
        await RemoveWithSmartPatternAsync(invocation);
        return result;
    }

    private async Task RemoveWithSmartPatternAsync(IInvocation invocation)
    {
        var culture = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

        string pattern = _cacheKeyPrefix switch
        {
            nameof(CacheKeys.UserResourcesPattern) => GetUserResourcesPattern(invocation, culture),
            nameof(CacheKeys.Role) => GetRolePattern(invocation, culture),
            _ => _cacheKeyPrefix  
        };

        var keys = await _cacheService!.SearchKeysAsync(pattern);
        foreach (var key in keys)
            await _cacheService.RemoveAsync(key);
    }

    private string GetUserResourcesPattern(IInvocation invocation, string culture)
    {
        var userId = invocation.Arguments.FirstOrDefault(arg => arg is Guid) as Guid?;
        if (userId == null)
            throw new BusinessRulesException("UserIdRequiredForUserResourcesPattern");
        return CacheKeys.UserResourcesPattern(userId.Value);
    }

    private string GetRolePattern(IInvocation invocation, string culture)
    {
        var roleId = invocation.Arguments.FirstOrDefault(arg => arg is int) as int?;
        if (roleId == null)
            throw new BusinessRulesException("RoleIdRequiredForRoleCacheInvalidation");
        return CacheKeys.Role(roleId.Value, culture);
    }
}
