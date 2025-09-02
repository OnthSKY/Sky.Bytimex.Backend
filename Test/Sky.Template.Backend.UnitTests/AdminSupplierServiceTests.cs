using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Suppliers;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities.Supplier;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminSupplierServiceTests
{
    private AdminSupplierService CreateService(Mock<ISupplierRepository> repo)
    {
        var uow = new Mock<IUnitOfWork>();
        var accessor = new Mock<IHttpContextAccessor>();
        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[]{ new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(ctx);
        return new AdminSupplierService(repo.Object, uow.Object, accessor.Object);
    }

    [Fact]
    public async Task CreateSupplierAsync_CreatesSupplier()
    {
        var repo = new Mock<ISupplierRepository>();
        var email = $"{Guid.NewGuid()}@example.com";
        var taxNumber = Guid.NewGuid().ToString();
        repo.Setup(r => r.IsEmailUniqueAsync(email, It.IsAny<Guid?>())).ReturnsAsync(true);
        repo.Setup(r => r.IsTaxNumberUniqueAsync(taxNumber, It.IsAny<Guid?>())).ReturnsAsync(true);
        repo.Setup(r => r.CreateAsync(It.IsAny<SupplierEntity>())).ReturnsAsync((SupplierEntity input) => input);
        var service = CreateService(repo);
        var req = new CreateSupplierRequest { Name = "Supp", Status = "ACTIVE", Email = email, TaxNumber = taxNumber };

        var res = await service.CreateSupplierAsync(req);

        res.Data.Should().NotBeNull();
        repo.Verify(r => r.CreateAsync(It.IsAny<SupplierEntity>()), Times.Once);
    }

    [Fact]
    public async Task GetSupplierByIdAsync_NotFoundThrows()
    {
        var repo = new Mock<ISupplierRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((SupplierEntity?)null);
        var service = CreateService(repo);

        Func<Task> act = () => service.GetSupplierByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteSupplierAsync_CallsRepository()
    {
        var repo = new Mock<ISupplierRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new SupplierEntity());
        var service = CreateService(repo);

        var result = await service.DeleteSupplierAsync(Guid.NewGuid());

        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        repo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Once);
    }
}
