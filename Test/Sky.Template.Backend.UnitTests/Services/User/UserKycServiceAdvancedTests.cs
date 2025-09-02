using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Kyc;
using Sky.Template.Backend.Contract.Responses.Kyc;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities.Kyc;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests.Services.User;

public class UserKycServiceAdvancedTests
{
    private UserKycService CreateUserService(Guid userId, Mock<IKycVerificationRepository> repo, Mock<ICacheService> cache)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(ctx);
        cache.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        cache.Setup(c => c.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<BaseControllerResponse<KycStatusResponse>>>>(), It.IsAny<CacheEntryOptions>()))
             .Returns<string, Func<Task<BaseControllerResponse<KycStatusResponse>>>, CacheEntryOptions>((k, f, o) => f());
        return new UserKycService(repo.Object, accessor.Object, cache.Object);
    }

    private AdminKycService CreateAdminService(IEnumerable<Claim> claims, Mock<IKycVerificationRepository> repo, Mock<ICacheService> cache)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var ctx = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) };
        accessor.Setup(a => a.HttpContext).Returns(ctx);
        cache.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        return new AdminKycService(repo.Object, accessor.Object, cache.Object);
    }

    [Fact]
    public async Task Submit_InvalidDocumentType_Throws()
    {
        var repo = new Mock<IKycVerificationRepository>();
        var cache = new Mock<ICacheService>();
        var service = CreateUserService(Guid.NewGuid(), repo, cache);
        var request = new KycVerificationRequest { DocumentType = "INVALID" };

        Func<Task> act = () => service.StartVerificationAsync(request);

        await act.Should().ThrowAsync<BusinessRulesException>();
        repo.Verify(r => r.CreateAsync(It.IsAny<KycVerificationEntity>()), Times.Never);
    }

    [Fact]
    public async Task GetStatus_CacheHitAndMiss()
    {
        var userId = Guid.NewGuid();
        var repo = new Mock<IKycVerificationRepository>();
        var entity = new KycVerificationEntity { Id = Guid.NewGuid(), UserId = userId };
        repo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(entity);
        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<BaseControllerResponse<KycStatusResponse>>>>(), It.IsAny<CacheEntryOptions>()))
             .Returns<string, Func<Task<BaseControllerResponse<KycStatusResponse>>>, CacheEntryOptions>((k, f, o) => f());
        var service = CreateUserService(userId, repo, cache);

        await service.GetStatusAsync(userId); // miss
        cache.Setup(c => c.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<BaseControllerResponse<KycStatusResponse>>>>(), It.IsAny<CacheEntryOptions>()))
             .ReturnsAsync(ControllerResponseBuilder.Success(new KycStatusResponse()));
        await service.GetStatusAsync(userId); // hit

        repo.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Approve_WithPermission_InvalidatesCache()
    {
        var repo = new Mock<IKycVerificationRepository>();
        var entity = new KycVerificationEntity { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        repo.Setup(r => r.GetByIdAsync(entity.Id)).ReturnsAsync(entity);
        repo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(entity);
        var cache = new Mock<ICacheService>();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()), new Claim(CustomClaimTypes.Permission, Permissions.Kyc.Verify) };
        var service = CreateAdminService(claims, repo, cache);
        var request = new KycApprovalRequest { VerificationId = entity.Id, Approve = true };

        var result = await service.ApproveVerificationAsync(request);

        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        cache.Verify(c => c.RemoveAsync(It.Is<string>(k => k.Contains(entity.UserId.ToString()))), Times.Once);
    }

    [Fact]
    public async Task Approve_WithoutPermission_ReturnsForbidden()
    {
        var repo = new Mock<IKycVerificationRepository>();
        var cache = new Mock<ICacheService>();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }; // no permission claim
        var service = CreateAdminService(claims, repo, cache);
        var request = new KycApprovalRequest
        {
            VerificationId = Guid.Parse("bbbb2222-2222-2222-2222-bbbbbbbbbbbb"),
            Approve = true
        };
        var result = await service.ApproveVerificationAsync(request);

        result.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        repo.Verify(r => r.UpdateAsync(It.IsAny<KycVerificationEntity>()), Times.Never);
    }

    [Fact]
    public async Task StartVerificationAsync_ConcurrentUpsert()
    {
        var userId = Guid.NewGuid();
        var repo = new Mock<IKycVerificationRepository>();
        var cache = new Mock<ICacheService>();
        KycVerificationEntity? existing = null;
        repo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(() => existing);
        repo.Setup(r => r.CreateAsync(It.IsAny<KycVerificationEntity>())).ReturnsAsync((KycVerificationEntity e) => { existing = e; return e; });
        repo.Setup(r => r.UpdateAsync(It.IsAny<KycVerificationEntity>())).ReturnsAsync((KycVerificationEntity e) => e);
        var service = CreateUserService(userId, repo, cache);
        var request = new KycVerificationRequest { DocumentType = "ID", NationalId = "1", Country = "TR", DocumentNumber = "123" };

        var t1 = service.StartVerificationAsync(request);
        var t2 = service.StartVerificationAsync(request);
        await Task.WhenAll(t1, t2);

        repo.Verify(r => r.CreateAsync(It.IsAny<KycVerificationEntity>()), Times.Once);
        repo.Verify(r => r.UpdateAsync(It.IsAny<KycVerificationEntity>()), Times.Once);
    }
}
