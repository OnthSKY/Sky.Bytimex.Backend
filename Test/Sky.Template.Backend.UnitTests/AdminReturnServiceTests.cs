using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Returns;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Application.Services;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminReturnServiceTests
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
    public async Task GetReturnsAsync_CallsRepository()
    {
        var repoMock = new Mock<IReturnRepository>();
        repoMock.Setup(r => r.GetFilteredPaginatedAsync(It.IsAny<ReturnFilterRequest>()))
            .ReturnsAsync((new List<ReturnEntity>(), 0));
        var auditMock = new Mock<IAuditLogService>();
        var service = new AdminReturnService(repoMock.Object, CreateAccessor(Guid.NewGuid()), auditMock.Object);

        await service.GetReturnsAsync(new ReturnFilterRequest());

        repoMock.Verify(r => r.GetFilteredPaginatedAsync(It.IsAny<ReturnFilterRequest>()), Times.Once);
    }

    [Fact]
    public async Task UpdateReturnStatusAsync_UpdatesEntity()
    {
        var entity = _fixture.Create<ReturnEntity>();
        var repoMock = new Mock<IReturnRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(entity);
        repoMock.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(entity);
        var accessor = CreateAccessor(Guid.NewGuid());
        var auditMock = new Mock<IAuditLogService>();
        var service = new AdminReturnService(repoMock.Object, accessor, auditMock.Object);
        var request = new UpdateReturnStatusRequest { Status = "APPROVED" };

        var response = await service.UpdateReturnStatusAsync(Guid.NewGuid(), request);

        response.Data!.Status.Should().Be("APPROVED");
        repoMock.Verify(r => r.UpdateAsync(entity), Times.Once);
    }

    [Fact]
    public async Task CreateReturnAsync_CallsRepository()
    {
        var repoMock = new Mock<IReturnRepository>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<ReturnEntity>())).ReturnsAsync(new ReturnEntity());
        var auditMock = new Mock<IAuditLogService>();
        var service = new AdminReturnService(repoMock.Object, CreateAccessor(Guid.NewGuid()), auditMock.Object);
        var request = _fixture.Create<CreateReturnRequest>();

        await service.CreateReturnAsync(request);

        repoMock.Verify(r => r.CreateAsync(It.IsAny<ReturnEntity>()), Times.Once);
    }

    [Fact]
    public async Task DeleteReturnAsync_DeletesEntity()
    {
        var returnId = Guid.NewGuid();
        var repoMock = new Mock<IReturnRepository>();
        repoMock.Setup(r => r.GetByIdAsync(returnId)).ReturnsAsync(new ReturnEntity { Id = returnId });
        repoMock.Setup(r => r.DeleteAsync(returnId)).ReturnsAsync(true);
        var auditMock = new Mock<IAuditLogService>();
        var service = new AdminReturnService(repoMock.Object, CreateAccessor(Guid.NewGuid()), auditMock.Object);

        await service.DeleteReturnAsync(returnId);

        repoMock.Verify(r => r.DeleteAsync(returnId), Times.Once);
    }
}

