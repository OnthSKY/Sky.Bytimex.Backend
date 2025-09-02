using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Returns;
using Sky.Template.Backend.Contract.Responses.ReturnResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.User;
using Xunit;
using System.Collections.Generic;

namespace Sky.Template.Backend.UnitTests;

public class UserReturnControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task CreateReturn_ReturnsCreated()
    {
        var serviceMock = new Mock<IUserReturnService>();
        var request = _fixture.Create<CreateReturnRequest>();
        var created = ControllerResponseBuilder.Success(new ReturnResponse(), "Created", HttpStatusCode.Created);
        serviceMock.Setup(s => s.CreateReturnAsync(request)).ReturnsAsync(created);
        var controller = new ReturnController(serviceMock.Object);

        var result = await controller.CreateReturn(request);

        result.Should().BeOfType<CreatedResult>();
        serviceMock.Verify(s => s.CreateReturnAsync(request), Times.Once);
    }

    [Fact]
    public async Task GetMyReturns_ReturnsOk()
    {
        var serviceMock = new Mock<IUserReturnService>();
        var response = ControllerResponseBuilder.Success<IEnumerable<ReturnResponse>>(new List<ReturnResponse>());
        serviceMock.Setup(s => s.GetMyReturnsAsync()).ReturnsAsync(response);
        var controller = new ReturnController(serviceMock.Object);

        var result = await controller.GetMyReturns();

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.GetMyReturnsAsync(), Times.Once);
    }

    [Fact]
    public async Task CancelReturn_ReturnsOk()
    {
        var serviceMock = new Mock<IUserReturnService>();
        var response = ControllerResponseBuilder.Success();
        serviceMock.Setup(s => s.CancelReturnAsync(It.IsAny<Guid>())).ReturnsAsync(response);
        var controller = new ReturnController(serviceMock.Object);

        var result = await controller.CancelReturn(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.CancelReturnAsync(It.IsAny<Guid>()), Times.Once);
    }
}

