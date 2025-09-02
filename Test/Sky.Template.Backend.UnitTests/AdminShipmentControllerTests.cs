using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Shipments;
using Sky.Template.Backend.Contract.Responses.ShipmentResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminShipmentControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task GetShipment_ReturnsOk()
    {
        var serviceMock = new Mock<IAdminShipmentService>();
        serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ControllerResponseBuilder.Success(new ShipmentResponse()));
        var controller = new ShipmentController(serviceMock.Object);

        var result = await controller.GetShipment(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task CreateShipment_ReturnsCreated()
    {
        var serviceMock = new Mock<IAdminShipmentService>();
        var request = _fixture.Create<CreateShipmentRequest>();
        serviceMock.Setup(s => s.CreateAsync(request))
            .ReturnsAsync(ControllerResponseBuilder.Success(new ShipmentResponse(), "Created", HttpStatusCode.Created));
        var controller = new ShipmentController(serviceMock.Object);

        var result = await controller.CreateShipment(request);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.StatusCode.Should().Be((int)HttpStatusCode.Created);
        serviceMock.Verify(s => s.CreateAsync(request), Times.Once);
    }

    [Fact]
    public async Task DeleteShipment_ReturnsOk()
    {
        var serviceMock = new Mock<IAdminShipmentService>();
        serviceMock.Setup(s => s.DeleteAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ControllerResponseBuilder.Success());
        var controller = new ShipmentController(serviceMock.Object);

        var result = await controller.DeleteShipment(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.DeleteAsync(It.IsAny<Guid>()), Times.Once);
    }
}
