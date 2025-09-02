using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Buyers;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminBuyerServiceTests
{
    private AdminBuyerService CreateService(Mock<IBuyerRepository> repo)
    {
        var uow = new Mock<IUnitOfWork>();
        var accessor = new Mock<IHttpContextAccessor>();
        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[]{ new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(ctx);
        return new AdminBuyerService(repo.Object, uow.Object, accessor.Object);
    }

    [Fact]
    public async Task CreateBuyerAsync_CreatesBuyer()
    {
        var repo = new Mock<IBuyerRepository>();

        var email = $"{Guid.NewGuid()}@example.com";
        var phone = "1";

        repo.Setup(r => r.IsEmailUniqueAsync(
                It.Is<string>(s => s.Equals(email, StringComparison.OrdinalIgnoreCase)),
                It.IsAny<Guid?>()))
            .ReturnsAsync(true);

        repo.Setup(r => r.IsPhoneUniqueAsync(
                It.Is<string>(p => p.EndsWith(phone)),  
                It.IsAny<Guid?>()))
            .ReturnsAsync(true);

        repo.Setup(r => r.CreateAsync(It.IsAny<BuyerEntity>()))
            .ReturnsAsync(new BuyerEntity
            {
                Id = Guid.NewGuid(),
                Name = "A",
                Email = email,
                Phone = phone
            });

        var service = CreateService(repo);
        var req = new CreateBuyerRequest { Name = "A", Email = email, Phone = phone };

        var res = await service.CreateBuyerAsync(req);

        res.Data.Should().NotBeNull();
        repo.Verify(r => r.IsEmailUniqueAsync(It.IsAny<string>(), It.IsAny<Guid?>()), Times.Once);
        repo.Verify(r => r.IsPhoneUniqueAsync(It.IsAny<string>(), It.IsAny<Guid?>()), Times.Once);
        repo.Verify(r => r.CreateAsync(It.IsAny<BuyerEntity>()), Times.Once);
    }



    [Fact]
    public async Task GetBuyerByIdAsync_NotFoundThrows()
    {
        var repo = new Mock<IBuyerRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((BuyerEntity?)null);
        var service = CreateService(repo);

        Func<Task> act = () => service.GetBuyerByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SoftDeleteBuyerAsync_CallsRepository()
    {
        var repo = new Mock<IBuyerRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new BuyerEntity());
        var service = CreateService(repo);

        var result = await service.SoftDeleteBuyerAsync(Guid.NewGuid());

        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        repo.Verify(r => r.UpdateAsync(It.IsAny<BuyerEntity>()), Times.Once);
    }
}
