using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Returns;
using Sky.Template.Backend.Core.Enums;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class UserReturnServiceTests
{
    private readonly Fixture _fixture = new();

    private static IHttpContextAccessor CreateAccessor(Guid userId)
    {
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(context);
        return accessor.Object;
    }

    [Fact]
    public async Task CreateReturnAsync_CallsRepository()
    {
        var repoMock = new Mock<IReturnRepository>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<ReturnEntity>())).ReturnsAsync(new ReturnEntity());
        var service = new UserReturnService(repoMock.Object, CreateAccessor(Guid.NewGuid()));
        var request = _fixture.Create<CreateReturnRequest>();

        await service.CreateReturnAsync(request);

        repoMock.Verify(r => r.CreateAsync(It.IsAny<ReturnEntity>()), Times.Once);
    }

    [Fact]
    public async Task GetReturnByIdAsync_ReturnsEntity()
    {
        var entity = _fixture.Create<ReturnEntity>();
        var repoMock = new Mock<IReturnRepository>();
        repoMock.Setup(r => r.GetByIdAsync(entity.Id)).ReturnsAsync(entity);
        var service = new UserReturnService(repoMock.Object, CreateAccessor(Guid.NewGuid()));

        var response = await service.GetReturnByIdAsync(entity.Id);

        response.Data.Should().NotBeNull();
        repoMock.Verify(r => r.GetByIdAsync(entity.Id), Times.Once);
    }

    [Fact]
    public async Task GetMyReturnsAsync_FiltersByUser()
    {
        var userId = Guid.NewGuid();
        var returns = new List<ReturnEntity>
        {
            new ReturnEntity { Id = Guid.NewGuid(), BuyerId = userId, Status = ReturnStatus.PENDING.ToString() },
            new ReturnEntity { Id = Guid.NewGuid(), BuyerId = Guid.NewGuid(), Status = ReturnStatus.PENDING.ToString() }
        };
        var repoMock = new Mock<IReturnRepository>();
        repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(returns);
        var service = new UserReturnService(repoMock.Object, CreateAccessor(userId));

        var response = await service.GetMyReturnsAsync();

        response.Data.Should().HaveCount(1);
        repoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CancelReturnAsync_Pending_SoftDeletes()
    {
        var userId = Guid.NewGuid();
        var returnId = Guid.NewGuid();
        var entity = new ReturnEntity { Id = returnId, BuyerId = userId, Status = ReturnStatus.PENDING.ToString() };
        var repoMock = new Mock<IReturnRepository>();
        repoMock.Setup(r => r.GetByIdAsync(returnId)).ReturnsAsync(entity);
        repoMock.Setup(r => r.SoftDeleteAsync(returnId, It.IsAny<string>())).ReturnsAsync(true);
        var service = new UserReturnService(repoMock.Object, CreateAccessor(userId));

        await service.CancelReturnAsync(returnId);

        repoMock.Verify(r => r.SoftDeleteAsync(returnId, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CancelReturnAsync_Approved_Throws()
    {
        var userId = Guid.NewGuid();
        var returnId = Guid.NewGuid();
        var entity = new ReturnEntity { Id = returnId, BuyerId = userId, Status = ReturnStatus.APPROVED.ToString() };
        var repoMock = new Mock<IReturnRepository>();
        repoMock.Setup(r => r.GetByIdAsync(returnId)).ReturnsAsync(entity);
        var service = new UserReturnService(repoMock.Object, CreateAccessor(userId));

        await Assert.ThrowsAsync<BusinessRulesException>(() => service.CancelReturnAsync(returnId));
    }
}

