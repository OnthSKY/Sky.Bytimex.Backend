using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Contract.Requests.Products;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.WebAPI.Controllers.Vendor;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class VendorProductControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task CreateProduct_ReturnsCreated()
    {
        var serviceMock = new Mock<IVendorProductService>();
        var request = _fixture.Create<CreateProductRequest>();
        var response = ControllerResponseBuilder.Success(new Sky.Template.Backend.Contract.Responses.ProductResponses.SingleProductResponse(), "Created", HttpStatusCode.Created);
        serviceMock.Setup(s => s.CreateProductAsync(request)).ReturnsAsync(response);
        var controller = new ProductController(serviceMock.Object);

        var result = await controller.CreateProduct(request);

        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.StatusCode.Should().Be((int)HttpStatusCode.Created);
        serviceMock.Verify(s => s.CreateProductAsync(request), Times.Once);
    }

    [Fact]
    public async Task GetFilteredProducts_ReturnsOk()
    {
        var serviceMock = new Mock<IVendorProductService>();
        var request = new ProductFilterRequest();
        var response = ControllerResponseBuilder.Success(new Sky.Template.Backend.Contract.Responses.ProductResponses.ProductListPaginatedDtoResponse());
        serviceMock.Setup(s => s.GetFilteredPaginatedProductsAsync(request)).ReturnsAsync(response);
        var controller = new ProductController(serviceMock.Object);

        var result = await controller.GetFilteredProducts(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        serviceMock.Verify(s => s.GetFilteredPaginatedProductsAsync(request), Times.Once);
    }

    [Fact]
    public async Task GetProductStockStatus_ReturnsOk()
    {
        var serviceMock = new Mock<IVendorProductService>();
        var response = ControllerResponseBuilder.Success(new Sky.Template.Backend.Contract.Responses.ProductResponses.ProductStockDto());
        serviceMock.Setup(s => s.GetProductStockStatusAsync(It.IsAny<Guid>())).ReturnsAsync(response);
        var controller = new ProductController(serviceMock.Object);

        var result = await controller.GetProductStockStatus(Guid.NewGuid());

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        serviceMock.Verify(s => s.GetProductStockStatusAsync(It.IsAny<Guid>()), Times.Once);
    }
}
