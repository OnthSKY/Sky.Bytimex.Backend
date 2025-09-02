using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.System;
using Sky.Template.Backend.Contract.Requests.FileUploads;
using Sky.Template.Backend.Contract.Responses.FileUploadResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.System;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class FileUploadControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task GetFileUploads_ReturnsOk()
    {
        var serviceMock = new Mock<IFileUploadService>();
        var response = ControllerResponseBuilder.Success(new List<FileUploadResponse>());
        serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(response);
        var controller = new FileUploadController(serviceMock.Object);

        var result = await controller.GetFileUploads();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        serviceMock.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateFileUpload_ReturnsCreated()
    {
        var serviceMock = new Mock<IFileUploadService>();
        var request = _fixture.Create<CreateFileUploadRequest>();
        var response = ControllerResponseBuilder.Success(new FileUploadResponse(), "Created", HttpStatusCode.Created);
        serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(response);
        var controller = new FileUploadController(serviceMock.Object);

        var result = await controller.CreateFileUpload(request);

        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.StatusCode.Should().Be((int)HttpStatusCode.Created);
        serviceMock.Verify(s => s.CreateAsync(request), Times.Once);
    }
}

