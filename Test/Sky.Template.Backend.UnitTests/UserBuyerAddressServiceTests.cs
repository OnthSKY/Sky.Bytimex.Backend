using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.BuyerAddresses;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class UserBuyerAddressServiceTests
{
    private readonly Fixture _fixture = new();

    private UserBuyerAddressService CreateService(Mock<IBuyerAddressRepository> repoMock)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(context);
        return new UserBuyerAddressService(repoMock.Object, accessor.Object);
    }

    [Fact]
    public async Task CreateAsync_CallsRepositoryCreate()
    {
        var repoMock = new Mock<IBuyerAddressRepository>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<BuyerAddressEntity>())).ReturnsAsync(new BuyerAddressEntity());
        repoMock.Setup(r => r.HasDefaultAsync(It.IsAny<Guid>(), null)).ReturnsAsync(false);
        var service = CreateService(repoMock);
        var request = _fixture.Build<CreateBuyerAddressRequest>().With(x => x.IsDefault, false).Create();

        await service.CreateAsync(request);

        repoMock.Verify(r => r.CreateAsync(It.IsAny<BuyerAddressEntity>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsAddress()
    {
        var repoMock = new Mock<IBuyerAddressRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new BuyerAddressEntity { Id = Guid.NewGuid(), BuyerId = Guid.NewGuid(), AddressTitle = "Title", FullAddress = "Full", City = "C", Country = "X", PostalCode = "1", CreatedAt = DateTime.UtcNow });
        var service = CreateService(repoMock);

        var response = await service.GetByIdAsync(Guid.NewGuid());

        response.Data.Should().NotBeNull();
        repoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }
}
