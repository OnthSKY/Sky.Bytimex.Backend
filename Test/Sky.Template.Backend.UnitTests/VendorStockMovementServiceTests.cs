using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities.Stock;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class VendorStockMovementServiceTests
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
    public async Task GetByIdAsync_OtherVendor_ThrowsNotFound()
    {
        var vendorId = Guid.NewGuid();
        var otherVendorId = Guid.NewGuid();
        var movement = new StockMovementEntity { Id = Guid.NewGuid(), SupplierId = otherVendorId, ProductId = Guid.NewGuid() };
        var movementRepo = new Mock<IStockMovementRepository>();
        movementRepo.Setup(r => r.GetByIdAsync(movement.Id)).ReturnsAsync(movement);
        var productRepo = new Mock<IProductRepository>();
        var service = new VendorStockMovementService(movementRepo.Object, productRepo.Object, CreateAccessor(vendorId));

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(movement.Id));
    }
}
