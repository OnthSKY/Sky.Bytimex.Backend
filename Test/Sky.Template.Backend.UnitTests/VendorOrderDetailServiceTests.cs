using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Contract.Requests.OrderDetails;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;
using ProductEntity = Sky.Template.Backend.Infrastructure.Entities.Product.ProductEntity;

namespace Sky.Template.Backend.UnitTests;

public class VendorOrderDetailServiceTests
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
    public async Task CreateAsync_CallsRepository_WhenOwner()
    {
        var vendorId = Guid.NewGuid();
        var orderRepo = new Mock<IOrderRepository>();
        orderRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new OrderEntity { VendorId = vendorId });
        var detailRepo = new Mock<IOrderDetailRepository>();
        detailRepo.Setup(r => r.CreateAsync(It.IsAny<OrderDetailEntity>())).ReturnsAsync(new OrderDetailEntity());
        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(p => p.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new ProductEntity { Status = "ACTIVE" });
        var service = new VendorOrderDetailService(detailRepo.Object, productRepo.Object, orderRepo.Object, CreateAccessor(vendorId));
        var request = _fixture.Create<CreateOrderDetailRequest>();

        await service.CreateAsync(request);

        detailRepo.Verify(r => r.CreateAsync(It.IsAny<OrderDetailEntity>()), Times.Once);
    }
}

