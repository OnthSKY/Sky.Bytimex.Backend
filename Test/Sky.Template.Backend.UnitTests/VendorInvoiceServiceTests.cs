using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Contract.Requests.Invoices;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class VendorInvoiceServiceTests
{
    private readonly Fixture _fixture = new();

    private VendorInvoiceService CreateService(Mock<IInvoiceRepository> repoMock)
    {
        var orderRepo = new Mock<IOrderRepository>();
        orderRepo.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new OrderEntity());
        var accessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(context);
        return new VendorInvoiceService(repoMock.Object, orderRepo.Object, accessor.Object);
    }

    [Fact]
    public async Task CreateAsync_CallsRepositoryCreate()
    {
        var repoMock = new Mock<IInvoiceRepository>();
        repoMock.Setup(r => r.GetByOrderIdAsync(It.IsAny<Guid>())).ReturnsAsync((InvoiceEntity?)null);
        repoMock.Setup(r => r.IsInvoiceNumberUniqueAsync(It.IsAny<string>(), null)).ReturnsAsync(true);
        repoMock.Setup(r => r.CreateAsync(It.IsAny<InvoiceEntity>())).ReturnsAsync(new InvoiceEntity());
        var service = CreateService(repoMock);
        var request = new CreateInvoiceRequest
        {
            OrderId = Guid.NewGuid(),
            InvoiceNumber = "INV-1",
            InvoiceDate = DateTime.UtcNow,
            BuyerId = Guid.NewGuid(),
            TotalAmount = 10,
            Currency = "TRY",
            Status = "ACTIVE"
        };

        await service.CreateAsync(request);

        repoMock.Verify(r => r.CreateAsync(It.IsAny<InvoiceEntity>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsInvoice()
    {
        var repoMock = new Mock<IInvoiceRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new InvoiceEntity
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            InvoiceNumber = "INV-1",
            InvoiceDate = DateTime.UtcNow,
            BuyerId = Guid.NewGuid(),
            TotalAmount = 5,
            Currency = "TRY",
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow
        });
        var service = CreateService(repoMock);

        var response = await service.GetByIdAsync(Guid.NewGuid());

        response.Data.Should().NotBeNull();
        repoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }
}
