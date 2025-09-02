using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Infrastructure.Entities.Product;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class UserProductCategoryServiceTests
{
    [Fact]
    public async Task GetAllCategoriesAsync_ReturnsList()
    {
        var repo = new Mock<IProductCategoryRepository>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ProductCategoryEntity>());
        var service = new UserProductCategoryService(repo.Object);

        var res = await service.GetAllCategoriesAsync();

        res.Data.Should().NotBeNull();
        repo.Verify(r => r.GetAllAsync(), Times.Once);
    }
}

