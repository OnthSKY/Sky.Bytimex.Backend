using System;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Shipments;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminShipmentServiceTests
{
    private readonly Fixture _fixture = new();

    private AdminShipmentService CreateService(Mock<IShipmentRepository> repoMock)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(context);
        return new AdminShipmentService(repoMock.Object, accessor.Object);
    }

    [Fact]
    public async Task CreateAsync_CallsRepositoryCreate()
    {
        var repoMock = new Mock<IShipmentRepository>();
        repoMock.Setup(r => r.GetByOrderIdAsync(It.IsAny<Guid>())).ReturnsAsync(Enumerable.Empty<ShipmentEntity>());
        repoMock.Setup(r => r.CreateAsync(It.IsAny<ShipmentEntity>())).ReturnsAsync(new ShipmentEntity());
        var service = CreateService(repoMock);
        var request = new CreateShipmentRequest
        {
            OrderId = Guid.NewGuid(),
            ShipmentDate = DateTime.UtcNow,
            Carrier = "UPS",
            TrackingNumber = "123",
            Status = "PENDING"
        };

        await service.CreateAsync(request);

        repoMock.Verify(r => r.CreateAsync(It.IsAny<ShipmentEntity>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsShipment()
    {
        var repoMock = new Mock<IShipmentRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new ShipmentEntity
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            ShipmentDate = DateTime.UtcNow,
            Carrier = "DHL",
            TrackingNumber = "321",
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow
        });
        var service = CreateService(repoMock);

        var response = await service.GetByIdAsync(Guid.NewGuid());

        response.Data.Should().NotBeNull();
        repoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }
}
