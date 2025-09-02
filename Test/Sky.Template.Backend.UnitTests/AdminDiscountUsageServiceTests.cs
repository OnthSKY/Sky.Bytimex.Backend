using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.DiscountUsages;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminDiscountUsageServiceTests
{
    private readonly Fixture _fixture = new();

    private AdminDiscountUsageService CreateService(Mock<IDiscountUsageRepository> usageRepo, Mock<IDiscountRepository> discountRepo)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]{ new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(context);
        return new AdminDiscountUsageService(usageRepo.Object, discountRepo.Object, accessor.Object);
    }

    [Fact]
    public async Task CreateDiscountUsageAsync_CreatesUsage()
    {
        var usageRepo = new Mock<IDiscountUsageRepository>();
        usageRepo.Setup(r => r.CreateAsync(It.IsAny<DiscountUsageEntity>())).ReturnsAsync(new DiscountUsageEntity());
        var discountRepo = new Mock<IDiscountRepository>();
        discountRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new DiscountEntity());
        var service = CreateService(usageRepo, discountRepo);
        var request = _fixture.Create<CreateDiscountUsageRequest>();

        await service.CreateDiscountUsageAsync(request);

        usageRepo.Verify(r => r.CreateAsync(It.IsAny<DiscountUsageEntity>()), Times.Once);
        discountRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }
}
