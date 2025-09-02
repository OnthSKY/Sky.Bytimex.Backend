using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Infrastructure.Entities.User;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class UserUserServiceTests
{
    private UserService CreateService(Mock<IUserRepository> repo)
    {
        var blob = new Mock<BlobServiceClient>();
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());
        return new UserService(repo.Object, blob.Object, accessor.Object);
    }

    [Fact]
    public async Task GetUserDtoByIdAsync_ReturnsUser()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetUserWithRoleByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new UserWithRoleEntity());
        var service = CreateService(repo);

        var res = await service.GetUserDtoByIdAsync(Guid.NewGuid());

        res.Data.Should().NotBeNull();
        repo.Verify(r => r.GetUserWithRoleByIdAsync(It.IsAny<Guid>()), Times.Once);
    }
}
