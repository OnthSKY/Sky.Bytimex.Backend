using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Permissions;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities.System;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;
using System.Security.Claims;

namespace Sky.Template.Backend.UnitTests;

public class AdminPermissionServiceTests
{
    private AdminPermissionService CreateService(Mock<IPermissionRepository> repo)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        }));
        accessor.Setup(a => a.HttpContext).Returns(ctx);
        return new AdminPermissionService(repo.Object, accessor.Object);
    }

    [Fact]
    public async Task CreatePermissionAsync_CreatesPermission()
    {
        var repo = new Mock<IPermissionRepository>();
        repo.Setup(r => r.CreateAsync(It.IsAny<PermissionEntity>())).ReturnsAsync(new PermissionEntity());
        var service = CreateService(repo);
        var req = new CreatePermissionRequest { Name = "perm" };

        var res = await service.CreatePermissionAsync(req);

        res.Data.Should().NotBeNull();
        repo.Verify(r => r.CreateAsync(It.IsAny<PermissionEntity>()), Times.Once);
    }

    [Fact]
    public async Task GetPermissionByIdAsync_NotFoundThrows()
    {
        var repo = new Mock<IPermissionRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((PermissionEntity?)null);
        var service = CreateService(repo);

        Func<Task> act = () => service.GetPermissionByIdAsync(1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeletePermissionAsync_CallsRepository()
    {
        var repo = new Mock<IPermissionRepository>();
        repo.Setup(r => r.DeleteAsync(It.IsAny<int>())).ReturnsAsync(true);
        var service = CreateService(repo);

        var result = await service.DeletePermissionAsync(1);

        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        repo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Once);
    }
}
