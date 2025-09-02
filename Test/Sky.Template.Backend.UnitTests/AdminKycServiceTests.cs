using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Kyc;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities.Kyc;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminKycServiceTests
{
    private AdminKycService CreateService(Mock<IKycVerificationRepository> repo, Mock<ICacheService>? cache = null)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[]{ new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(ctx);
        var cacheMock = cache ?? new Mock<ICacheService>();
        cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        return new AdminKycService(repo.Object, accessor.Object, cacheMock.Object);
    }

    [Fact]
    public async Task ApproveVerificationAsync_NotFound_Throws()
    {
        var repo = new Mock<IKycVerificationRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((KycVerificationEntity?)null);
        var service = CreateService(repo);
        var request = new KycApprovalRequest { VerificationId = Guid.NewGuid(), Approve = true };

        Func<Task> act = () => service.ApproveVerificationAsync(request);

        await act.Should().ThrowAsync<NotFoundException>();
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
}

