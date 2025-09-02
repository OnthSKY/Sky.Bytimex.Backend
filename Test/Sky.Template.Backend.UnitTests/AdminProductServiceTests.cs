using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminProductServiceTests
{
    private static AdminProductService CreateService(Mock<IProductRepository> repo)
    {
        var translationRepo = new Mock<IProductTranslationRepository>();
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());
        return new AdminProductService(repo.Object, translationRepo.Object, accessor.Object);
    }

    [Fact]
    public async Task HardDeleteProductAsync_WhenProductDoesNotExist_ShouldThrowNotFoundException()
    {
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Sky.Template.Backend.Infrastructure.Entities.Product.ProductEntity?)null);
        var service = CreateService(repo);

        Func<Task> act = () => service.HardDeleteProductAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
