using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.OrderStatusLogs;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminOrderStatusLogServiceTests
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
    public async Task CreateAsync_CallsRepository()
    {
        var logRepo = new Mock<IOrderStatusLogRepository>();
        logRepo.Setup(r => r.CreateAsync(It.IsAny<OrderStatusLogEntity>())).ReturnsAsync(new OrderStatusLogEntity());
        var orderRepo = new Mock<IOrderRepository>();
        orderRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new OrderEntity());
        var service = new AdminOrderStatusLogService(logRepo.Object, orderRepo.Object, CreateAccessor(Guid.NewGuid()));
        var request = _fixture.Create<CreateOrderStatusLogRequest>();

        await service.CreateAsync(request);

        logRepo.Verify(r => r.CreateAsync(It.IsAny<OrderStatusLogEntity>()), Times.Once);
    }

    [Fact]
    public async Task GetByOrderIdAsync_ReturnsLogs()
    {
        var logs = _fixture.CreateMany<OrderStatusLogEntity>(2);
        var logRepo = new Mock<IOrderStatusLogRepository>();
        logRepo.Setup(r => r.GetByOrderIdAsync(It.IsAny<Guid>())).ReturnsAsync(logs);
        var orderRepo = new Mock<IOrderRepository>();
        orderRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new OrderEntity());
        var service = new AdminOrderStatusLogService(logRepo.Object, orderRepo.Object, CreateAccessor(Guid.NewGuid()));

        var response = await service.GetByOrderIdAsync(Guid.NewGuid());

        response.Data!.Logs.Should().HaveCount(2);
    }
}
