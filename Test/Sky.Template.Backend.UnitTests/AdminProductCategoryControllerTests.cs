using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Products;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.ProductResponses;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminProductCategoryControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task GetCategoryById_ReturnsOk_WhenFound()
    {
        var serviceMock = new Mock<IAdminProductCategoryService>();
        var response = ControllerResponseBuilder.Success(new SingleProductCategoryResponse());
        serviceMock.Setup(s => s.GetCategoryByIdAsync(It.IsAny<Guid>())).ReturnsAsync(response);
        var controller = new ProductCategoryController(serviceMock.Object);

        var result = await controller.GetById(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.GetCategoryByIdAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCategory_ReturnsBadRequest_WhenInvalidData()
    {
        var serviceMock = new Mock<IAdminProductCategoryService>();
        var request = _fixture.Create<UpdateProductCategoryRequest>();
        var bad = ControllerResponseBuilder.Failure<SingleProductCategoryResponse>("Invalid", HttpStatusCode.BadRequest);
        serviceMock.Setup(s => s.UpdateCategoryAsync(It.IsAny<Guid>(), request)).ReturnsAsync(bad);
        var controller = new ProductCategoryController(serviceMock.Object);

        var result = await controller.Update(Guid.NewGuid(), request);

        result.Should().BeOfType<BadRequestObjectResult>();
        serviceMock.Verify(s => s.UpdateCategoryAsync(It.IsAny<Guid>(), request), Times.Once);
    }
}
