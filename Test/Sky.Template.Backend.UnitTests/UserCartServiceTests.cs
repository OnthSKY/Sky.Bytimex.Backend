using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Carts;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class UserCartServiceTests
{
    private readonly Fixture _fixture = new();

    private UserCartService CreateService(Mock<ICartRepository> repoMock)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(context);
        return new UserCartService(repoMock.Object, accessor.Object);
    }

    [Fact]
    public async Task CreateCartAsync_InsertsCart()
    {
        var repoMock = new Mock<ICartRepository>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<CartEntity>())).ReturnsAsync(new CartEntity());
        var service = CreateService(repoMock);
        var request = _fixture.Create<CreateCartRequest>();

        await service.CreateAsync(request);

        repoMock.Verify(r => r.CreateAsync(It.IsAny<CartEntity>()), Times.Once);
    }

    [Fact]
    public async Task GetCartById_ReturnsCart()
    {
        var repoMock = new Mock<ICartRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new CartEntity { Id = Guid.NewGuid(), BuyerId = Guid.NewGuid(), Currency = "USD", Status = "OPEN", CreatedAt = DateTime.UtcNow });
        var service = CreateService(repoMock);

        var response = await service.GetByIdAsync(Guid.NewGuid());

        response.Data.Should().NotBeNull();
        repoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }
}
