using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Kyc;
using Sky.Template.Backend.Contract.Responses.Kyc;
using Sky.Template.Backend.Infrastructure.Entities.Kyc;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class UserKycServiceTests
{
    private UserKycService CreateService(Mock<IKycVerificationRepository> repo, Mock<ICacheService>? cache = null)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[]{ new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(ctx);
        var cacheMock = cache ?? new Mock<ICacheService>();
        cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        cacheMock.Setup(c => c.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<BaseControllerResponse<KycStatusResponse>>>>(), It.IsAny<CacheEntryOptions>()))
            .Returns<string, Func<Task<BaseControllerResponse<KycStatusResponse>>>, CacheEntryOptions>((k, f, o) => f());
        return new UserKycService(repo.Object, accessor.Object, cacheMock.Object);
    }

    [Fact]
    public async Task StartVerificationAsync_CreatesRecord()
    {
        var repo = new Mock<IKycVerificationRepository>();
        repo.Setup(r => r.CreateAsync(It.IsAny<KycVerificationEntity>()))
            .ReturnsAsync((KycVerificationEntity e) => e);
        var service = CreateService(repo);
        var request = new KycVerificationRequest { NationalId = "1", Country = "TR", DocumentType = "ID", DocumentNumber = "123" };

        var response = await service.StartVerificationAsync(request);

        response.Data.Should().NotBeNull();
        repo.Verify(r => r.CreateAsync(It.IsAny<KycVerificationEntity>()), Times.Once);
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsStatus()
    {
        var repo = new Mock<IKycVerificationRepository>();
        var entity = new KycVerificationEntity { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), NationalId = "1", Country = "TR", DocumentType = "ID", DocumentNumber = "123" };
        repo.Setup(r => r.GetByUserIdAsync(entity.UserId)).ReturnsAsync(entity);
        var service = CreateService(repo);

        var result = await service.GetStatusAsync(entity.UserId);

        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ResubmitKycAsync_UpdatesRecord_WhenRejected()
    {
        var userId = Guid.NewGuid();
        var repo = new Mock<IKycVerificationRepository>();
        var entity = new KycVerificationEntity { Id = Guid.NewGuid(), UserId = userId, Status = "REJECTED" };
        repo.Setup(r => r.GetByIdAsync(entity.Id)).ReturnsAsync(entity);
        repo.Setup(r => r.UpdateAsync(It.IsAny<KycVerificationEntity>())).ReturnsAsync(entity);
        var service = CreateService(repo);
        var request = new KycSubmissionRequest { VerificationId = entity.Id, NationalId = "1", Country = "TR", DocumentType = "ID", DocumentNumber = "123" };

        await service.ResubmitKycAsync(userId, request);

        repo.Verify(r => r.UpdateAsync(It.Is<KycVerificationEntity>(e => e.Status == "PENDING")), Times.Once);
    }

    [Fact]
    public async Task DeleteKycAsync_SoftDeletes_WhenPending()
    {
        var userId = Guid.NewGuid();
        var repo = new Mock<IKycVerificationRepository>();
        var entity = new KycVerificationEntity { Id = Guid.NewGuid(), UserId = userId, Status = "PENDING" };
        repo.Setup(r => r.GetByIdAsync(entity.Id)).ReturnsAsync(entity);
        repo.Setup(r => r.UpdateAsync(It.IsAny<KycVerificationEntity>())).ReturnsAsync(entity);
        var service = CreateService(repo);

        await service.DeleteKycAsync(userId, entity.Id);

        repo.Verify(r => r.UpdateAsync(It.Is<KycVerificationEntity>(e => e.IsDeleted)), Times.Once);
    }
}

