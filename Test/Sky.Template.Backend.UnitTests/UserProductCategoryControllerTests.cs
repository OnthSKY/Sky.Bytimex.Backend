using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.ProductResponses;
using Sky.Template.Backend.WebAPI.Controllers.User;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class UserProductCategoryControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var serviceMock = new Mock<IUserProductCategoryService>();
        serviceMock.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(ControllerResponseBuilder.Success(new ProductCategoryListResponse()));
        var controller = new ProductCategoryController(serviceMock.Object);

        var result = await controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.GetAllCategoriesAsync(), Times.Once);
    }
}
