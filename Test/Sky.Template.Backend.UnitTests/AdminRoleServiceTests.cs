using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Roles;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities.UserRole;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;
using System.Security.Claims;

namespace Sky.Template.Backend.UnitTests;

public class AdminRoleServiceTests
{
    private AdminRoleService CreateService(Mock<IRoleRepository> repo)
    {
        var permRepo = new Mock<IPermissionRepository>();
        var accessor = new Mock<IHttpContextAccessor>();
        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        }));
        accessor.Setup(a => a.HttpContext).Returns(ctx);
        var helper = new Mock<IUserRoleHelperService>();
        return new AdminRoleService(repo.Object, permRepo.Object, accessor.Object, helper.Object);
    }

    [Fact]
    public async Task CreateRoleAsync_CreatesRole()
    {
        var repo = new Mock<IRoleRepository>();
        repo.Setup(r => r.CreateAsync(It.IsAny<RoleEntity>())).ReturnsAsync(new RoleEntity());
        var service = CreateService(repo);
        var req = new CreateRoleRequest { Name = "Admin" };

        var res = await service.CreateRoleAsync(req);

        res.Data.Should().NotBeNull();
        repo.Verify(r => r.CreateAsync(It.IsAny<RoleEntity>()), Times.Once);
    }

    [Fact]
    public async Task GetRoleByIdAsync_NotFoundThrows()
    {
        var repo = new Mock<IRoleRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((RoleEntity?)null);
        var service = CreateService(repo);

        Func<Task> act = () => service.GetRoleByIdAsync(1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SoftDeleteRoleAsync_CallsRepository()
    {
        var repo = new Mock<IRoleRepository>();
        repo.Setup(r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(true);
        var service = CreateService(repo);

        var result = await service.SoftDeleteRoleAsync(1);

        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        repo.Verify(r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }
}
