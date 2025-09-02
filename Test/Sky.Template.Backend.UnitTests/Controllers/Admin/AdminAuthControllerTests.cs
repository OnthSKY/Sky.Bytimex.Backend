using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Auth;
using Sky.Template.Backend.Contract.Responses.Auth;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Configs;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Sky.Template.Backend.UnitTests.Common;
using Xunit;

namespace Sky.Template.Backend.UnitTests.Controllers.Admin;

public class AdminAuthControllerTests
{
    private readonly Mock<IAdminAuthService> _service = new();
    private readonly AzureAdLoginConfig _config = new()
    {
        ClientId = "client",
        TenantId = "tenant",
        ClientSecret = "secret",
        RedirectUrl = "https://callback",
        ScopeLink = "scope",
        AuthorityLink = "https://login"
    };

    private AdminAuthController CreateController(ClaimsPrincipal? user = null)
    {
        var controller = new AdminAuthController(_service.Object, Options.Create(_config));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user ?? TestClaimsPrincipalFactory.CreateDefault()
            }
        };
        return controller;
    }

    [Fact]
    public async Task Impersonate_ReturnsOk_OnSuccess()
    {
        var request = new ImpersonateRequest { Email = "user@local" };
        _service.Setup(s => s.ImpersonateAsync(request))
            .ReturnsAsync(ControllerResponseBuilder.Success(new AuthResponse()));
        var controller = CreateController();

        var result = await controller.Impersonate(request);

        result.Should().BeOfType<OkObjectResult>();
        _service.Verify(s => s.ImpersonateAsync(request), Times.Once);
    }

    [Fact]
    public async Task Impersonate_Throws_WhenServiceThrows()
    {
        var request = new ImpersonateRequest { Email = "user@local" };
        _service.Setup(s => s.ImpersonateAsync(request)).ThrowsAsync(new Exception("fail"));
        var controller = CreateController();

        var action = async () => await controller.Impersonate(request);

        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public void Impersonate_HasExpectedAttributes()
    {
        var method = typeof(AdminAuthController).GetMethod(nameof(AdminAuthController.Impersonate));
        method.Should().NotBeNull();
        method!.GetCustomAttributes(typeof(HttpPostAttribute), false).Should().ContainSingle()
            .Which.As<HttpPostAttribute>().Template.Should().Be("api/auth/impersonations");
        method.GetCustomAttributes(typeof(AuthorizeAttribute), false).Should().ContainSingle()
            .Which.As<AuthorizeAttribute>().Policy.Should().Be("Admin");
    }

    [Fact]
    public void GetImpersonationStatus_ReturnsOk()
    {
        var adminId = Guid.NewGuid();
        GlobalImpersonationContext.AdminId = adminId;
        var userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, "user@local"),
            new Claim(CustomClaimTypes.ImpersonatedBy, adminId.ToString())
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var controller = CreateController(principal);

        var result = controller.GetImpersonationStatus();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        dynamic value = ok.Value!;
        ((Guid?)value.AdminId).Should().Be(adminId);
        ((bool)value.IsImpersonating).Should().BeTrue();
        GlobalImpersonationContext.AdminId = GlobalImpersonationContext.GetSentinelValue();
    }

    [Fact]
    public void GetImpersonationStatus_HasExpectedAttributes()
    {
        var method = typeof(AdminAuthController).GetMethod(nameof(AdminAuthController.GetImpersonationStatus));
        method.Should().NotBeNull();
        method!.GetCustomAttributes(typeof(HttpGetAttribute), false).Should().ContainSingle()
            .Which.As<HttpGetAttribute>().Template.Should().Be("api/auth/impersonations/status");
        method.GetCustomAttributes(typeof(AuthorizeAttribute), false).Should().ContainSingle();
    }

    [Fact]
    public async Task ReturnUser_ReturnsOk()
    {
        _service.Setup(s => s.ReturnUserAsync())
            .ReturnsAsync(ControllerResponseBuilder.Success(new AuthResponse()));
        var controller = CreateController();

        var result = await controller.ReturnUser();

        result.Should().BeOfType<OkObjectResult>();
        _service.Verify(s => s.ReturnUserAsync(), Times.Once);
    }

    [Fact]
    public void ReturnUser_HasExpectedAttributes()
    {
        var method = typeof(AdminAuthController).GetMethod(nameof(AdminAuthController.ReturnUser));
        method.Should().NotBeNull();
        method!.GetCustomAttributes(typeof(HttpPostAttribute), false).Should().ContainSingle()
            .Which.As<HttpPostAttribute>().Template.Should().Be("api/auth/impersonations/revert");
        method.GetCustomAttributes(typeof(AuthorizeAttribute), false).Should().ContainSingle()
            .Which.As<AuthorizeAttribute>().Policy.Should().Be("Admin");
    }

    [Fact]
    public void RedirectToAdLogin_ReturnsOk()
    {
        var controller = CreateController();

        var result = controller.RedirectToAdLogin();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<BaseControllerResponse<AzureLinkResponse>>();
    }

    [Fact]
    public void RedirectToAdLogin_HasExpectedAttributes()
    {
        var method = typeof(AdminAuthController).GetMethod(nameof(AdminAuthController.RedirectToAdLogin));
        method.Should().NotBeNull();
        method!.GetCustomAttributes(typeof(HttpGetAttribute), false).Should().ContainSingle()
            .Which.As<HttpGetAttribute>().Template.Should().Be("api/auth/sign-in/azure");
    }

    [Fact]
    public async Task LoginWithAD_ReturnsOk()
    {
        _service.Setup(s => s.LoginWithADAsync("code", null, null))
            .ReturnsAsync(ControllerResponseBuilder.Success(new AuthResponse()));
        var controller = CreateController();

        var result = await controller.LoginWithAD("code", null, null);

        result.Should().BeOfType<OkObjectResult>();
        _service.Verify(s => s.LoginWithADAsync("code", null, null), Times.Once);
    }

    [Fact]
    public void LoginWithAD_HasExpectedAttributes()
    {
        var method = typeof(AdminAuthController).GetMethod(nameof(AdminAuthController.LoginWithAD));
        method.Should().NotBeNull();
        method!.GetCustomAttributes(typeof(RouteAttribute), false).Should().ContainSingle()
            .Which.As<RouteAttribute>().Template.Should().Be("signin-oidc");
        method.GetCustomAttributes(typeof(AllowAnonymousAttribute), false).Should().ContainSingle();
        method.GetCustomAttributes(typeof(HttpGetAttribute), false).Should().ContainSingle();
    }
}
