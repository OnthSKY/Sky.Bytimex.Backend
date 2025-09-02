using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Xunit;

namespace Sky.Template.Backend.UnitTests.Controllers.Admin;

public class AdminUserControllerTests
{
    [Fact]
    public void Controller_HasAdminAuthorizeAttribute()
    {
        var attr = typeof(AdminUserController).BaseType?.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("Admin", attr!.Roles);
    }
}
