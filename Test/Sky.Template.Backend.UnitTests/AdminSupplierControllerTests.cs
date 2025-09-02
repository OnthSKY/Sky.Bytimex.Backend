using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Suppliers;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.SupplierResponses;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminSupplierControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task GetAllSuppliers_ReturnsOk()
    {
        var serviceMock = new Mock<IAdminSupplierService>();
        var response = ControllerResponseBuilder.Success(new SupplierListResponse());
        serviceMock.Setup(s => s.GetAllSuppliersAsync()).ReturnsAsync(response);
        var controller = new SupplierController(serviceMock.Object);

        var result = await controller.GetAllSuppliers();

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.GetAllSuppliersAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateSupplier_ReturnsBadRequest_WhenInvalidData()
    {
        var serviceMock = new Mock<IAdminSupplierService>();
        var request = _fixture.Create<CreateSupplierRequest>();
        var bad = ControllerResponseBuilder.Failure<SingleSupplierResponse>("Invalid", HttpStatusCode.BadRequest);
        serviceMock.Setup(s => s.CreateSupplierAsync(request)).ReturnsAsync(bad);
        var controller = new SupplierController(serviceMock.Object);

        var result = await controller.CreateSupplier(request);

        result.Should().BeOfType<BadRequestObjectResult>();
        serviceMock.Verify(s => s.CreateSupplierAsync(request), Times.Once);
    }

    [Fact]
    public async Task DeleteSupplier_ReturnsNotFound_WhenServiceReturnsNotFound()
    {
        var serviceMock = new Mock<IAdminSupplierService>();
        var notFound = new BaseControllerResponse { StatusCode = HttpStatusCode.NotFound, Message = "NotFound" };
        serviceMock.Setup(s => s.DeleteSupplierAsync(It.IsAny<Guid>())).ReturnsAsync(notFound);
        var controller = new SupplierController(serviceMock.Object);

        var result = await controller.DeleteSupplier(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>();
        serviceMock.Verify(s => s.DeleteSupplierAsync(It.IsAny<Guid>()), Times.Once);
    }
}
