using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Infrastructure.Entities.System;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class UserResourceServiceTests
{
    [Fact]
    public async Task GetCurrentUserResourcesAsync_ReturnsResources()
    {
        var repo = new Mock<IResourceRepository>();
        repo.Setup(r => r.GetByUserAsync(It.IsAny<Guid>())).ReturnsAsync(new List<ResourceEntity>());

        var accessor = new Mock<IHttpContextAccessor>();
        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(ctx);

        var service = new UserResourceService(repo.Object, accessor.Object);
        var result = await service.GetCurrentUserResourcesAsync();

        result.Data.Should().NotBeNull();
        repo.Verify(r => r.GetByUserAsync(It.IsAny<Guid>()), Times.Once);
    }
}
