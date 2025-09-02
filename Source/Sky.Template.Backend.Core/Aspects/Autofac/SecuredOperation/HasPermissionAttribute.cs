using System;
using System.Linq;
using System.Security.Claims;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Helpers;
using Sky.Template.Backend.Core.Utilities.Interceptors;

namespace Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class HasPermissionAttribute : MethodInterception
{
    private readonly string[] _requiredPermissions;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HasPermissionAttribute(string permissions)
    {
        _requiredPermissions = permissions.Split(',').Select(p => p.Trim()).ToArray();
        _httpContextAccessor = ServiceTool.ServiceProvider.GetService<IHttpContextAccessor>()
                               ?? throw new Exception("IHttpContextAccessor resolve edilemedi.");
    }

    protected override void OnBefore(IInvocation invocation)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
            throw new ForbiddenException("Auth.Unauthorized");

        var userPermissions = user.Claims
            .Where(c => c.Type == CustomClaimTypes.Permission)
            .Select(c => c.Value)
            .ToList();

        if (_requiredPermissions.Any(rp => userPermissions.Contains(rp)))
            return;

        throw new ForbiddenException("Auth.PermissionDenied");
    }
} 