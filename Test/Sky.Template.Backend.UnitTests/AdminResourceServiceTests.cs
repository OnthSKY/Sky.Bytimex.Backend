using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Resources;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities.System;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminResourceServiceTests
{
    private AdminResourceService CreateService(Mock<IResourceRepository> repo)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[]{ new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(ctx);
        return new AdminResourceService(repo.Object, accessor.Object);
    }

    [Fact]
    public async Task CreateAsync_CreatesResource()
    {
        var repo = new Mock<IResourceRepository>();
        repo.Setup(r => r.CreateAsync(It.IsAny<ResourceEntity>())).ReturnsAsync(new ResourceEntity());
        var service = CreateService(repo);
        var req = new CreateResourceRequest { Code = "CODE", Name = "Res" };

        var res = await service.CreateAsync(req);

        res.Data.Should().NotBeNull();
        repo.Verify(r => r.CreateAsync(It.IsAny<ResourceEntity>()), Times.Once);
    }

    [Fact]
    public async Task GetByCodeAsync_NotFoundThrows()
    {
        var repo = new Mock<IResourceRepository>();
        repo.Setup(r => r.GetByCodeAsync(It.IsAny<string>())).ReturnsAsync((ResourceEntity?)null);
        var service = CreateService(repo);

        Func<Task> act = () => service.GetByCodeAsync("code");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_CallsRepository()
    {
        var repo = new Mock<IResourceRepository>();
        repo.Setup(r => r.DeleteAsync(It.IsAny<string>())).ReturnsAsync(true);
        var service = CreateService(repo);

        var result = await service.DeleteAsync("code");

        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        repo.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Once);
    }
}
