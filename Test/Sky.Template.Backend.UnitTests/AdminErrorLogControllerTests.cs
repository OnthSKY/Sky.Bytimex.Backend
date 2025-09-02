using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.ErrorLogs;
using Sky.Template.Backend.Contract.Responses.ErrorLogResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminErrorLogControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task GetLogs_ReturnsOk()
    {
        var serviceMock = new Mock<IAdminErrorLogService>();
        var response = ControllerResponseBuilder.Success(new ErrorLogListPaginatedResponse());
        serviceMock.Setup(s => s.GetAllAsync(It.IsAny<GridRequest>())).ReturnsAsync(response);
        var controller = new ErrorLogController(serviceMock.Object);

        var result = await controller.GetLogs(new GridRequest());

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        serviceMock.Verify(s => s.GetAllAsync(It.IsAny<GridRequest>()), Times.Once);
    }

    [Fact]
    public async Task GetLogById_ReturnsNotFound_WhenServiceReturnsNotFound()
    {
        var serviceMock = new Mock<IAdminErrorLogService>();
        var notFound = ControllerResponseBuilder.Failure<ErrorLogResponse>("NotFound", HttpStatusCode.NotFound);
        serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(notFound);
        var controller = new ErrorLogController(serviceMock.Object);

        var result = await controller.GetLogById(Guid.NewGuid());

        var nfResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        nfResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        serviceMock.Verify(s => s.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var serviceMock = new Mock<IAdminErrorLogService>();
        var controller = new ErrorLogController(serviceMock.Object);
        var request = _fixture.Create<CreateErrorLogRequest>();

        var result = await controller.Create(request);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.StatusCode.Should().Be((int)HttpStatusCode.Created);
        serviceMock.Verify(s => s.LogErrorAsync(It.IsAny<CreateErrorLogRequest>()), Times.Once);
    }
}
