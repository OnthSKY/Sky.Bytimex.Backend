using System;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Contract.Requests.Shipments;
using Sky.Template.Backend.Core.Exceptions;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class VendorShipmentServiceTests
{
    private VendorShipmentService CreateService(Mock<IShipmentRepository> repoMock, Guid vendorId)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, vendorId.ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(context);
        return new VendorShipmentService(repoMock.Object, accessor.Object);
    }

    [Fact]
    public async Task GetByIdAsync_OtherVendor_ThrowsNotFound()
    {
        var vendorId = Guid.NewGuid();
        var repoMock = new Mock<IShipmentRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new ShipmentEntity { Id = Guid.NewGuid(), CreatedBy = Guid.NewGuid() });
        var service = CreateService(repoMock, vendorId);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetByIdAsync_OwnShipment_ReturnsShipment()
    {
        var vendorId = Guid.NewGuid();
        var repoMock = new Mock<IShipmentRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new ShipmentEntity
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            ShipmentDate = DateTime.UtcNow,
            Carrier = "DHL",
            TrackingNumber = "1",
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = vendorId
        });
        var service = CreateService(repoMock, vendorId);

        var response = await service.GetByIdAsync(Guid.NewGuid());

        response.Data.Should().NotBeNull();
    }
}
