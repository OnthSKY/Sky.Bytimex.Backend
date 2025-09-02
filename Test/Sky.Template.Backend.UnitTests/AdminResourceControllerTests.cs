using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Resources;
using Sky.Template.Backend.Contract.Responses.ResourceResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminResourceControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task GetResources_ReturnsOkAsync()
    {
        var serviceMock = new Mock<IAdminResourceService>();
        var response = ControllerResponseBuilder.Success(new ResourceListResponse());
        serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(response);
        var controller = new ResourceController(serviceMock.Object);

        var result = await controller.GetResourcesAsync();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        serviceMock.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetResourceByCode_ReturnsNotFound_WhenServiceReturnsNotFoundAsync()
    {
        var serviceMock = new Mock<IAdminResourceService>();
        var notFound = ControllerResponseBuilder.Failure<SingleResourceResponse>("NotFound", HttpStatusCode.NotFound);
        serviceMock.Setup(s => s.GetByCodeAsync(It.IsAny<string>())).ReturnsAsync(notFound);
        var controller = new ResourceController(serviceMock.Object);

        var result = await controller.GetResourceByCodeAsync("code");

        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        serviceMock.Verify(s => s.GetByCodeAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateResource_ReturnsCreated_WhenValidInputAsync()
    {
        var serviceMock = new Mock<IAdminResourceService>();
        var request = _fixture.Create<CreateResourceRequest>();
        var response = ControllerResponseBuilder.Success(new SingleResourceResponse(), "Created", HttpStatusCode.Created);
        serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(response);
        var controller = new ResourceController(serviceMock.Object);

        var result = await controller.CreateResourceAsync(request);

        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.StatusCode.Should().Be((int)HttpStatusCode.Created);
        serviceMock.Verify(s => s.CreateAsync(request), Times.Once);
    }

    [Fact]
    public async Task UpdateResource_ReturnsBadRequest_WhenInvalidDataAsync()
    {
        var serviceMock = new Mock<IAdminResourceService>();
        var request = _fixture.Create<UpdateResourceRequest>();
        var badRequest = ControllerResponseBuilder.Failure<SingleResourceResponse>("Invalid", HttpStatusCode.BadRequest);
        serviceMock.Setup(s => s.UpdateAsync(It.IsAny<string>(), request)).ReturnsAsync(badRequest);
        var controller = new ResourceController(serviceMock.Object);

        var result = await controller.UpdateResourceAsync("code", request);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        serviceMock.Verify(s => s.UpdateAsync(It.IsAny<string>(), request), Times.Once);
    }

    [Fact]
    public async Task DeleteResource_ReturnsOk_WhenSuccessfulAsync()
    {
        var serviceMock = new Mock<IAdminResourceService>();
        var okResponse = ControllerResponseBuilder.Success();
        serviceMock.Setup(s => s.DeleteAsync(It.IsAny<string>())).ReturnsAsync(okResponse);
        var controller = new ResourceController(serviceMock.Object);

        var result = await controller.DeleteResourceAsync("code");

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        serviceMock.Verify(s => s.DeleteAsync(It.IsAny<string>()), Times.Once);
    }
}

