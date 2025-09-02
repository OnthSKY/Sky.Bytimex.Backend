using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Roles;
using Sky.Template.Backend.Contract.Requests.Users;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Models;
using Sky.Template.Backend.Infrastructure.Entities.User;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminUserServiceTests
{
    private AdminUserService CreateService(Mock<IUserRepository> repo)
    {
        var roleHelper = new Mock<IUserRoleHelperService>();
        var accessor = new Mock<IHttpContextAccessor>();
        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[]{ new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(ctx);
        return new AdminUserService(repo.Object, roleHelper.Object, accessor.Object);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsData()
    {
        var repo = new Mock<IUserRepository>();
        var list = new List<UserWithRoleEntity> { new() { Id = Guid.NewGuid(), Name = "A", Surname = "B", Email = "e", ImagePath="", RoleId=1, RoleName="R", RoleDescription="D", PermissionNamesRaw="" } };
        repo.Setup(r => r.GetAllUsersWithFilterAsync(It.IsAny<UsersFilterRequest>())).ReturnsAsync((list.AsEnumerable(), list.Count));
        var service = CreateService(repo);

        var result = await service.GetAllUsersAsync(new UsersFilterRequest());

        result.Data.Should().NotBeNull();
        repo.Verify(r => r.GetAllUsersWithFilterAsync(It.IsAny<UsersFilterRequest>()), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteUserAsync_CallsRepository()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetUserWithRoleByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new UserWithRoleEntity());
        repo.Setup(r => r.SoftDeleteUserAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(true);
        var service = CreateService(repo);

        var res = await service.SoftDeleteUserAsync(Guid.NewGuid());

        res.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        repo.Verify(r => r.SoftDeleteUserAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
    }
}
