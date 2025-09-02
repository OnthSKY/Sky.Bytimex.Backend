using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Linq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.CartItems;
using Sky.Template.Backend.Contract.Responses.CartItemResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.User;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class UserCartItemControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task GetCartItems_ReturnsOk()
    {
        var serviceMock = new Mock<IUserCartItemService>();
        var response = ControllerResponseBuilder.Success(Enumerable.Empty<CartItemResponse>());
        serviceMock.Setup(s => s.GetByCartIdAsync(It.IsAny<Guid>())).ReturnsAsync(response);
        var controller = new CartItemController(serviceMock.Object);

        var result = await controller.GetCartItems(Guid.NewGuid());

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateCartItem_ReturnsCreated()
    {
        var serviceMock = new Mock<IUserCartItemService>();
        var request = _fixture.Create<CreateCartItemRequest>();
        var response = ControllerResponseBuilder.Success(new CartItemResponse(), "Created", HttpStatusCode.Created);
        serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(response);
        var controller = new CartItemController(serviceMock.Object);

        var result = await controller.CreateCartItem(request);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.StatusCode.Should().Be((int)HttpStatusCode.Created);
    }
}
