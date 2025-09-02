using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.StockMovements;
using Sky.Template.Backend.Infrastructure.Entities.Product;
using Sky.Template.Backend.Infrastructure.Entities.Stock;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminStockMovementServiceTests
{
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
    public async Task CreateAsync_UpdatesProductStock()
    {
        var movementRepo = new Mock<IStockMovementRepository>();
        movementRepo.Setup(r => r.CreateAsync(It.IsAny<StockMovementEntity>())).ReturnsAsync(new StockMovementEntity());
        var product = new ProductEntity { Id = Guid.NewGuid(), StockQuantity = 10 };
        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
        productRepo.Setup(r => r.UpdateAsync(product)).ReturnsAsync(product);
        var accessor = CreateAccessor(Guid.NewGuid());
        var service = new AdminStockMovementService(movementRepo.Object, productRepo.Object, accessor);
        var request = new CreateStockMovementRequest
        {
            ProductId = product.Id,
            MovementType = "IN",
            Quantity = 5
        };

        await service.CreateAsync(request);

        productRepo.Verify(r => r.UpdateAsync(It.Is<ProductEntity>(p => p.StockQuantity == 15)), Times.Once);
        movementRepo.Verify(r => r.CreateAsync(It.IsAny<StockMovementEntity>()), Times.Once);
    }
}
