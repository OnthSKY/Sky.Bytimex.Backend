using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Buyers;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.SaleResponses;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Xunit;

namespace Sky.Template.Backend.UnitTests;
public class AdminBuyerControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task GetBuyerById_ReturnsOk_WhenFound()
    {
        var serviceMock = new Mock<IAdminBuyerService>();
        var response = ControllerResponseBuilder.Success(new SingleBuyerResponse());
        serviceMock.Setup(s => s.GetBuyerByIdAsync(It.IsAny<Guid>())).ReturnsAsync(response);
        var controller = new BuyerController(serviceMock.Object);

        var result = await controller.GetBuyerById(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.GetBuyerByIdAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task CreateBuyer_ReturnsCreated_WhenValid()
    {
        var serviceMock = new Mock<IAdminBuyerService>();
        var request = _fixture.Create<CreateBuyerRequest>();
        var created = ControllerResponseBuilder.Success(new SingleBuyerResponse(), "Created", HttpStatusCode.Created);
        serviceMock.Setup(s => s.CreateBuyerAsync(request)).ReturnsAsync(created);
        var controller = new BuyerController(serviceMock.Object);

        var result = await controller.CreateBuyer(request);

        result.Should().BeOfType<CreatedResult>();
        serviceMock.Verify(s => s.CreateBuyerAsync(request), Times.Once);
    }

    [Fact]
    public async Task UpdateBuyer_ReturnsNotFound_WhenServiceReturnsNotFound()
    {
        var serviceMock = new Mock<IAdminBuyerService>();
        var request = _fixture.Create<UpdateBuyerRequest>();
        var notFound = ControllerResponseBuilder.Failure<SingleBuyerResponse>("NotFound", HttpStatusCode.NotFound);
        serviceMock.Setup(s => s.UpdateBuyerAsync(It.IsAny<Guid>(), request)).ReturnsAsync(notFound);
        var controller = new BuyerController(serviceMock.Object);

        var result = await controller.UpdateBuyer(Guid.NewGuid(), request);

        result.Should().BeOfType<NotFoundObjectResult>();
        serviceMock.Verify(s => s.UpdateBuyerAsync(It.IsAny<Guid>(), request), Times.Once);
    }
}
