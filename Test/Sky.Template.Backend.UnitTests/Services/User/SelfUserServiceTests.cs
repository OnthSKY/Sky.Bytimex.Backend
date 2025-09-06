using AutoFixture;
using FluentAssertions;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Users;
using Sky.Template.Backend.Contract.Responses.UserResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Infrastructure.Entities.User;
using Sky.Template.Backend.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Azure.Storage.Blobs;
using Xunit;

namespace Sky.Template.Backend.UnitTests.Services.User;

public class SelfUserServiceTests
{
    private readonly Fixture _fixture = new();

    private UserService CreateService(Mock<IUserRepository> repo)
    {
        var blob = new Mock<BlobServiceClient>();
        var accessor = new Mock<IHttpContextAccessor>();
        return new UserService(repo.Object, blob.Object, accessor.Object);
    }

    [Fact]
    public async Task GetSelfProfileAsync_WhenUserExists_ReturnsComposedProfile()
    {
        var repo = new Mock<IUserRepository>();
        var profile = _fixture.Create<SelfProfileResponse>();
        var userId = Guid.NewGuid();
        repo.Setup(r => r.GetSelfProfileAsync(userId)).ReturnsAsync(profile);

        var service = CreateService(repo);

        var result = await service.GetSelfProfileAsync(userId);

        result.Data.Should().BeEquivalentTo(profile);
        repo.Verify(r => r.GetSelfProfileAsync(userId), Times.Once);
    }

    [Fact]
    public async Task UpdateSelfProfileAsync_WhenValid_UpdatesAllowedFields()
    {
        var repo = new Mock<IUserRepository>();
        var request = _fixture.Create<SelfUpdateProfileRequest>();
        var userId = Guid.NewGuid();
        var entity = new UserEntity { Id = userId, FirstName = request.FirstName, LastName = request.LastName, Email = "a@b.com" };
        repo.Setup(r => r.UpdateSelfProfileAsync(userId, request)).ReturnsAsync(entity);

        var service = CreateService(repo);

        var result = await service.UpdateSelfProfileAsync(userId, request);

        result.Data.User.FirstName.Should().Be(request.FirstName);
        result.Data.User.LastName.Should().Be(request.LastName);
        repo.Verify(r => r.UpdateSelfProfileAsync(userId, request), Times.Once);
    }

    [Fact]
    public async Task GetSelfPermissionsAsync_ReturnsFlattenedPermissionCodes()
    {
        var repo = new Mock<IUserRepository>();
        var codes = new List<string> { "a", "b" };
        var userId = Guid.NewGuid();
        repo.Setup(r => r.GetSelfPermissionCodesAsync(userId)).ReturnsAsync(codes);
        var service = CreateService(repo);

        var result = await service.GetSelfPermissionsAsync(userId);

        result.Data.Should().BeEquivalentTo(codes);
    }

    [Fact]
    public async Task RevokeSelfSessionAsync_RemovesSessionOfCurrentUserOnly()
    {
        var repo = new Mock<IUserRepository>();
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        repo.Setup(r => r.RevokeSelfSessionAsync(userId, sessionId)).ReturnsAsync(true);
        var service = CreateService(repo);

        var result = await service.RevokeSelfSessionAsync(userId, sessionId);

        result.Data.Should().BeTrue();
        repo.Verify(r => r.RevokeSelfSessionAsync(userId, sessionId), Times.Once);
    }
}
