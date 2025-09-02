using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Contract.Requests.Payments;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class VendorPaymentServiceTests
{
    private readonly Fixture _fixture = new();

    private VendorPaymentService CreateService(Mock<IPaymentRepository> repoMock)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(context);
        return new VendorPaymentService(repoMock.Object, accessor.Object);
    }

    [Fact]
    public async Task CreateAsync_CallsRepositoryCreate()
    {
        var repoMock = new Mock<IPaymentRepository>();
        repoMock.Setup(r => r.GetByOrderIdAsync(It.IsAny<Guid>())).ReturnsAsync(Enumerable.Empty<PaymentEntity>());
        repoMock.Setup(r => r.CreateAsync(It.IsAny<PaymentEntity>())).ReturnsAsync(new PaymentEntity());
        var service = CreateService(repoMock);
        var request = new CreatePaymentRequest
        {
            OrderId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            Amount = 10,
            PaymentType = "CASH",
            PaymentStatus = "PENDING"
        };

        await service.CreateAsync(request);

        repoMock.Verify(r => r.CreateAsync(It.IsAny<PaymentEntity>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPayment()
    {
        var repoMock = new Mock<IPaymentRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new PaymentEntity
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            Amount = 5,
            Currency = "TRY",
            PaymentType = "CASH",
            PaymentStatus = "PENDING",
            CreatedAt = DateTime.UtcNow
        });
        var service = CreateService(repoMock);

        var response = await service.GetByIdAsync(Guid.NewGuid());

        response.Data.Should().NotBeNull();
        repoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }
}
