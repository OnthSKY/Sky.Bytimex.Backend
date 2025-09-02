using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Contract.Requests.Vendors;
using Sky.Template.Backend.Contract.Responses.VendorResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.Vendor;
using Xunit;

namespace Sky.Template.Backend.UnitTests.Controllers.Vendor;

public class VendorSelfControllerTests
{
    [Fact]
    public async Task UpdateVendor_ReturnsOk()
    {
        var serviceMock = new Mock<IVendorSelfService>();
        serviceMock.Setup(s => s.UpdateVendorAsync(It.IsAny<UpdateVendorRequest>()))
            .ReturnsAsync(ControllerResponseBuilder.Success(new SingleVendorResponse()));
        var controller = new VendorSelfController(serviceMock.Object);

        var result = await controller.UpdateVendor(Guid.NewGuid(), new UpdateVendorRequest());

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.UpdateVendorAsync(It.IsAny<UpdateVendorRequest>()), Times.Once);
    }

    [Fact]
    public async Task GetVerificationStatus_ReturnsOk()
    {
        var serviceMock = new Mock<IVendorSelfService>();
        serviceMock.Setup(s => s.GetVerificationStatusAsync())
            .ReturnsAsync(ControllerResponseBuilder.Success(new VerificationStatusDto()));
        var controller = new VendorSelfController(serviceMock.Object);

        var result = await controller.GetVerificationStatus();

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.GetVerificationStatusAsync(), Times.Once);
    }
}

