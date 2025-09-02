using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Responses.ShipmentResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.User;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class UserShipmentControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task TrackShipment_ReturnsOk_WhenFound()
    {
        var serviceMock = new Mock<IUserShipmentService>();
        var trackingNumber = _fixture.Create<string>();
        serviceMock.Setup(s => s.TrackShipmentAsync(trackingNumber))
            .ReturnsAsync(ControllerResponseBuilder.Success(new ShipmentResponse()));
        var controller = new ShipmentController(serviceMock.Object);

        var result = await controller.TrackShipment(trackingNumber);

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.TrackShipmentAsync(trackingNumber), Times.Once);
    }

    [Fact]
    public async Task TrackShipment_ReturnsNotFound_WhenServiceReturnsNotFound()
    {
        var serviceMock = new Mock<IUserShipmentService>();
        var trackingNumber = _fixture.Create<string>();
        var notFound = ControllerResponseBuilder.Failure<ShipmentResponse>("NotFound", HttpStatusCode.NotFound);
        serviceMock.Setup(s => s.TrackShipmentAsync(trackingNumber)).ReturnsAsync(notFound);
        var controller = new ShipmentController(serviceMock.Object);

        var result = await controller.TrackShipment(trackingNumber);

        result.Should().BeOfType<NotFoundObjectResult>();
        serviceMock.Verify(s => s.TrackShipmentAsync(trackingNumber), Times.Once);
    }
}
