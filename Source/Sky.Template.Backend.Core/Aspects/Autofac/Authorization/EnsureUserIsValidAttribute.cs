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

        var userServiceType = Type.GetType("Sky.Template.Backend.Application.Services.IUserService, Sky.Template.Backend.Application");
        if (userServiceType != null)
        {
            _userService = ServiceTool.ServiceProvider.GetService(userServiceType);
            _isActiveMethod = userServiceType.GetMethod("IsActive");
        }
    }

    protected override void OnBefore(IInvocation invocation)
    {
        foreach (var argument in invocation.Arguments)
        {
            if (argument == null) continue;

            var argType = argument.GetType();

            foreach (var field in _fields)
            {
                var property = argType.GetProperty(field);
                if (property == null)
                {
                    throw new BusinessRulesException("PropertyNotFoundInRequest", field);
                }

                var value = property.GetValue(argument);
                if (value == null || value is Guid guid && guid == Guid.Empty)
                {
                    throw new BusinessRulesException("PropertyIsRequired", field);
                }

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
                    {
                        throw new BusinessRulesException("UserIdNotActive", userId);
                    }
                }
            }
        }
    }
}
