using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Discounts;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminDiscountServiceTests
{
    private readonly Fixture _fixture = new();

    private AdminDiscountService CreateService(Mock<IDiscountRepository> repoMock)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]{ new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(context);
        var uowMock = new Mock<IUnitOfWork>();
        return new AdminDiscountService(repoMock.Object, uowMock.Object, accessor.Object);
    }

    [Fact]
    public async Task CreateDiscountAsync_InsertsDiscount()
    {
        var repoMock = new Mock<IDiscountRepository>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<DiscountEntity>())).ReturnsAsync(new DiscountEntity());
        var service = CreateService(repoMock);
        var request = _fixture.Create<CreateDiscountRequest>();

        await service.CreateDiscountAsync(request);

        repoMock.Verify(r => r.CreateAsync(It.IsAny<DiscountEntity>()), Times.Once);
    }

    [Fact]
    public async Task GetDiscountById_ReturnsDiscount()
    {
        var repoMock = new Mock<IDiscountRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new DiscountEntity { Id = Guid.NewGuid(), Code = "X", CreatedAt = DateTime.UtcNow });
        var service = CreateService(repoMock);

        var response = await service.GetDiscountByIdAsync(Guid.NewGuid());

        response.Data.Should().NotBeNull();
        repoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }
}
