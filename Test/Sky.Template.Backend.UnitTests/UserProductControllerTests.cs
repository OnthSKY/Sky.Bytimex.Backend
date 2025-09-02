using System;
using System.Net;
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

public class UserProductControllerTests
{
    [Fact]
    public async Task GetAllProducts_ReturnsOk()
    {
        var serviceMock = new Mock<IUserProductService>();
        serviceMock.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(ControllerResponseBuilder.Success(new ProductListResponse()));
        var controller = new ProductController(serviceMock.Object);

        var result = await controller.GetAllProducts();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be((int)HttpStatusCode.OK);
        serviceMock.Verify(s => s.GetAllProductsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetProductStockStatus_ReturnsOk()
    {
        var serviceMock = new Mock<IUserProductService>();
        serviceMock.Setup(s => s.GetProductStockStatusAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ControllerResponseBuilder.Success(new ProductStockDto()));
        var controller = new ProductController(serviceMock.Object);

        var result = await controller.GetProductStockStatus(Guid.NewGuid());

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be((int)HttpStatusCode.OK);
        serviceMock.Verify(s => s.GetProductStockStatusAsync(It.IsAny<Guid>()), Times.Once);
    }
}
