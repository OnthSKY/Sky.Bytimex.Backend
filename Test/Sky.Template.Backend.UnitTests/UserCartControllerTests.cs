using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Carts;
using Sky.Template.Backend.Contract.Responses.CartResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.User;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class UserCartControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task GetCartById_ReturnsOk()
    {
        var serviceMock = new Mock<IUserCartService>();
        var response = ControllerResponseBuilder.Success(new CartResponse());
        serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(response);
        var controller = new CartController(serviceMock.Object);

        var result = await controller.GetCart(Guid.NewGuid());

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateCart_ReturnsCreated()
    {
        var serviceMock = new Mock<IUserCartService>();
        var request = _fixture.Create<CreateCartRequest>();
        var response = ControllerResponseBuilder.Success(new CartResponse(), "Created", HttpStatusCode.Created);
        serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(response);
        var controller = new CartController(serviceMock.Object);

        var result = await controller.CreateCart(request);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.StatusCode.Should().Be((int)HttpStatusCode.Created);
    }
}
