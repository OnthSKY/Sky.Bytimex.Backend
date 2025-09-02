using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.OrderDetails;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;
using ProductEntity = Sky.Template.Backend.Infrastructure.Entities.Product.ProductEntity;

namespace Sky.Template.Backend.UnitTests;

public class AdminOrderDetailServiceTests
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
        var repoMock = new Mock<IOrderDetailRepository>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<OrderDetailEntity>())).ReturnsAsync(new OrderDetailEntity());
        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(p => p.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new ProductEntity { Status = "ACTIVE" });
        var service = new AdminOrderDetailService(repoMock.Object, productRepo.Object, CreateAccessor(Guid.NewGuid()));
        var request = _fixture.Create<CreateOrderDetailRequest>();

        await service.CreateAsync(request);

        repoMock.Verify(r => r.CreateAsync(It.IsAny<OrderDetailEntity>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsData()
    {
        var entity = _fixture.Create<OrderDetailEntity>();
        var repoMock = new Mock<IOrderDetailRepository>();
        repoMock.Setup(r => r.GetByIdAsync(entity.Id)).ReturnsAsync(entity);
        var productRepo = new Mock<IProductRepository>();
        var service = new AdminOrderDetailService(repoMock.Object, productRepo.Object, CreateAccessor(Guid.NewGuid()));

        var response = await service.GetByIdAsync(entity.Id);

        response.Data!.Id.Should().Be(entity.Id);
    }
}

