using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class UserProductServiceTests
{
    [Fact]
    public async Task GetProductByIdAsync_NotFoundThrows()
    {
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Sky.Template.Backend.Infrastructure.Entities.Product.ProductEntity?)null);
        var trRepo = new Mock<IProductTranslationRepository>();
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());
        var service = new UserProductService(repo.Object, trRepo.Object, accessor.Object);

        Func<Task> act = () => service.GetProductByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetProductStockStatusAsync_NotFoundThrows()
    {
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Sky.Template.Backend.Infrastructure.Entities.Product.ProductEntity?)null);
        var trRepo = new Mock<IProductTranslationRepository>();
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());
        var service = new UserProductService(repo.Object, trRepo.Object, accessor.Object);

        Func<Task> act = () => service.GetProductStockStatusAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
