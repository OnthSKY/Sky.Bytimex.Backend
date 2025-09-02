using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.BuyerAddresses;
using Sky.Template.Backend.Contract.Responses.BuyerAddressResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.User;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class UserBuyerAddressControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task GetBuyerAddress_ReturnsOk()
    {
        var serviceMock = new Mock<IUserBuyerAddressService>();
        serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(ControllerResponseBuilder.Success(new BuyerAddressResponse()));
        var controller = new BuyerAddressController(serviceMock.Object);

        var result = await controller.GetBuyerAddress(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task CreateBuyerAddress_ReturnsCreated()
    {
        var serviceMock = new Mock<IUserBuyerAddressService>();
        var request = _fixture.Create<CreateBuyerAddressRequest>();
        serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(ControllerResponseBuilder.Success(new BuyerAddressResponse(), "Created", HttpStatusCode.Created));
        var controller = new BuyerAddressController(serviceMock.Object);

        var result = await controller.CreateBuyerAddress(request);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.StatusCode.Should().Be((int)HttpStatusCode.Created);
        serviceMock.Verify(s => s.CreateAsync(request), Times.Once);
    }

    [Fact]
    public async Task DeleteBuyerAddress_ReturnsOk()
    {
        var serviceMock = new Mock<IUserBuyerAddressService>();
        serviceMock.Setup(s => s.SoftDeleteAsync(It.IsAny<Guid>())).ReturnsAsync(ControllerResponseBuilder.Success());
        var controller = new BuyerAddressController(serviceMock.Object);

        var result = await controller.DeleteBuyerAddress(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.SoftDeleteAsync(It.IsAny<Guid>()), Times.Once);
    }
}
