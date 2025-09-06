using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Helpers;
using Sky.Template.Backend.Core.Utilities.Interceptors;

namespace Sky.Template.Backend.Core.Aspects.Autofac.Authorization;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class EnsureUserIsValidAttribute : MethodInterception
{
    private readonly string[] _fields;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly object? _userService;
    private readonly MethodInfo? _isActiveMethod;

    public EnsureUserIsValidAttribute(string[] fields)
    {
        _fields = fields;
        _httpContextAccessor = ServiceTool.ServiceProvider.GetService<IHttpContextAccessor>()
                               ?? throw new BusinessRulesException("HttpContextAccessorNotResolved");

        var userServiceType =
            Type.GetType("Sky.Template.Backend.Application.Services.IUserService, Sky.Template.Backend.Application");
        if (userServiceType != null)
        {
            _userService = ServiceTool.ServiceProvider.GetService(userServiceType);
            _isActiveMethod = userServiceType.GetMethod("IsActive");
        }
    }
    protected override void OnBefore(IInvocation invocation)
    {
        var paramInfos = invocation.Method.GetParameters();
        var fieldSet = new HashSet<string>(_fields ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < invocation.Arguments.Length; i++)
        {
            var argument = invocation.Arguments[i];
            if (argument == null) continue;

            var argType = argument.GetType();
            var paramName = paramInfos[i].Name ?? string.Empty;

            // 1) Eğer bu parametrenin adı _fields içinde ise ve argüman primitive/Guid ise,
            //    değeri doğrudan bu argümandan al.
            if (fieldSet.Contains(paramName) && (argType.IsPrimitive || argType == typeof(Guid) || argType == typeof(string)))
            {
                ValidateValue(argument, paramName);
                continue;
            }

            // 2) Değilse; fieldSet’teki her field’ı önce property olarak (case-insensitive) ara
            foreach (var field in fieldSet)
            {
                var property = argType.GetProperty(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (property == null)
                {
                    // Bu argüman içinde bulunamadı; diğer field/arg combo’larına bakmaya devam et
                    continue;
                }

                var value = property.GetValue(argument);
                ValidateValue(value, field);
            }
        }
    }

    private void ValidateValue(object? value, string fieldOrParamName)
    {
        if (value == null)
            throw new BusinessRulesException("PropertyIsRequired", fieldOrParamName);

        if (value is Guid g && g == Guid.Empty)
            throw new BusinessRulesException("PropertyIsRequired", fieldOrParamName);

        if (_userService != null && _isActiveMethod != null && value is Guid userId)
        {
            var result = _isActiveMethod.Invoke(_userService, new object[] { userId });
            bool isActive = result switch
            {
                bool b => b,
                Task<bool> task => task.ConfigureAwait(false).GetAwaiter().GetResult(),
                _ => true
            };
            if (!isActive)
                throw new BusinessRulesException("UserIdNotActive", userId);
        }
    }

}
