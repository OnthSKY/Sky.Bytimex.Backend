using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Vendors;
using Sky.Template.Backend.Contract.Responses.VendorResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Xunit;

namespace Sky.Template.Backend.UnitTests.Controllers.Admin;

public class VendorControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task GetVendors_ReturnsOk()
    {
        var serviceMock = new Mock<IAdminVendorService>();
        var response = ControllerResponseBuilder.Success(new PaginatedVendorListResponse());
        serviceMock.Setup(s => s.GetFilteredPaginatedVendorsAsync(It.IsAny<VendorFilterRequest>()))
            .ReturnsAsync(response);
        var controller = new VendorController(serviceMock.Object);

        var result = await controller.GetFilteredPaginatedVendors(new VendorFilterRequest());

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.GetFilteredPaginatedVendorsAsync(It.IsAny<VendorFilterRequest>()), Times.Once);
    }

    [Fact]
    public async Task CreateVendor_ReturnsBadRequest_WhenInvalid()
    {
        var serviceMock = new Mock<IAdminVendorService>();
        var request = _fixture.Create<CreateVendorRequest>();
        var bad = ControllerResponseBuilder.Failure<SingleVendorResponse>("Invalid", HttpStatusCode.BadRequest);
        serviceMock.Setup(s => s.CreateVendorAsync(request)).ReturnsAsync(bad);
        var controller = new VendorController(serviceMock.Object);

        var result = await controller.CreateVendor(request);

        result.Should().BeOfType<BadRequestObjectResult>();
        serviceMock.Verify(s => s.CreateVendorAsync(request), Times.Once);
    }

    [Fact]
    public async Task ApproveVendor_ReturnsOk()
    {
        var serviceMock = new Mock<IAdminVendorService>();
        serviceMock.Setup(s => s.ApproveVendorAsync(It.IsAny<Guid>(), It.IsAny<string?>()))
            .ReturnsAsync(ControllerResponseBuilder.Success());
        var controller = new VendorController(serviceMock.Object);

        var result = await controller.ApproveVendor(Guid.NewGuid(), null);

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.ApproveVendorAsync(It.IsAny<Guid>(), It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task RejectVendor_ReturnsOk()
    {
        var serviceMock = new Mock<IAdminVendorService>();
        serviceMock.Setup(s => s.RejectVendorAsync(It.IsAny<Guid>(), It.IsAny<string?>()))
            .ReturnsAsync(ControllerResponseBuilder.Success());
        var controller = new VendorController(serviceMock.Object);

        var result = await controller.RejectVendor(Guid.NewGuid(), null);

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.RejectVendorAsync(It.IsAny<Guid>(), It.IsAny<string?>()), Times.Once);
    }
}

