using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Orders;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminOrderServiceTests
{
    private AdminOrderService CreateService(Mock<IOrderRepository> repo)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(ctx);
        return new AdminOrderService(repo.Object, accessor.Object);
    }

    [Fact]
    public async Task CreateSaleAsync_WhenRequestIsValid_ShouldCreateSale()
    {
        var repo = new Mock<IOrderRepository>();
        repo.Setup(r => r.CreateAsync(It.IsAny<OrderEntity>())).ReturnsAsync((OrderEntity input) => input);
        var service = CreateService(repo);
        var req = new CreateOrderRequest { VendorId = Guid.NewGuid(), TotalAmount = 10, Currency = "USD", SaleStatus = "NEW", OrderDate = DateTime.UtcNow };

        var res = await service.CreateSaleAsync(req);

        res.Data.Should().NotBeNull();
        repo.Verify(r => r.CreateAsync(It.IsAny<OrderEntity>()), Times.Once);
    }

    [Fact]
    public async Task GetSaleByIdAsync_WhenOrderDoesNotExist_ShouldThrowNotFoundException()
    {
        var repo = new Mock<IOrderRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((OrderEntity?)null);
        var service = CreateService(repo);

        Func<Task> act = () => service.GetSaleByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SoftDeleteSaleAsync_WhenOrderExists_ShouldUpdateRepository()
    {
        var repo = new Mock<IOrderRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new OrderEntity());
        var service = CreateService(repo);

        var res = await service.SoftDeleteSaleAsync(Guid.NewGuid());

        res.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        repo.Verify(r => r.UpdateAsync(It.IsAny<OrderEntity>()), Times.Once);
    }
}
