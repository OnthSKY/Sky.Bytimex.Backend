using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Products;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities.Product;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminProductCategoryServiceTests
{
    private AdminProductCategoryService CreateService(Mock<IProductCategoryRepository> repo)
    {
        var uow = new Mock<IUnitOfWork>();
        var accessor = new Mock<IHttpContextAccessor>();
        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[]{ new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(ctx);
        return new AdminProductCategoryService(repo.Object, uow.Object, accessor.Object);
    }

    [Fact]
    public async Task CreateCategoryAsync_CreatesCategory()
    {
        var repo = new Mock<IProductCategoryRepository>();
        repo.Setup(r => r.IsCategoryNameUniqueAsync(It.IsAny<string>(), null)).ReturnsAsync(true);
        repo.Setup(r => r.CreateAsync(It.IsAny<ProductCategoryEntity>()))
            .ReturnsAsync((ProductCategoryEntity input) => input);  
        var service = CreateService(repo);
        var req = new CreateProductCategoryRequest { Name = "Cat" };

        var res = await service.CreateCategoryAsync(req);

        res.Data.Should().NotBeNull();
        repo.Verify(r => r.CreateAsync(It.IsAny<ProductCategoryEntity>()), Times.Once);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_NotFoundThrows()
    {
        var repo = new Mock<IProductCategoryRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ProductCategoryEntity?)null);
        var service = CreateService(repo);

        Func<Task> act = () => service.GetCategoryByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SoftDeleteCategoryAsync_CallsRepository()
    {
        var repo = new Mock<IProductCategoryRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new ProductCategoryEntity());
        var service = CreateService(repo);

        var result = await service.SoftDeleteCategoryAsync(Guid.NewGuid());

        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        repo.Verify(r => r.UpdateAsync(It.IsAny<ProductCategoryEntity>()), Times.Once);
    }
}
