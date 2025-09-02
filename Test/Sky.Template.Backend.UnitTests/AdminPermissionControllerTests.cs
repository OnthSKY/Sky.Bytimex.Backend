using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Permissions;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.PermissionResponses;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Xunit;

namespace Sky.Template.Backend.UnitTests;
public class AdminPermissionControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task CreatePermission_ReturnsCreated_WhenValid()
    {
        var mock = new Mock<IAdminPermissionService>();
        var request = _fixture.Create<CreatePermissionRequest>();
        var response = ControllerResponseBuilder.Success(new SinglePermissionResponse(), "Created", HttpStatusCode.Created);
        mock.Setup(s => s.CreatePermissionAsync(request)).ReturnsAsync(response);
        var controller = new PermissionController(mock.Object);

        var result = await controller.CreatePermission(request);

        result.Should().BeOfType<CreatedResult>();
        mock.Verify(s => s.CreatePermissionAsync(request), Times.Once);
    }

    [Fact]
    public async Task DeletePermission_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var mock = new Mock<IAdminPermissionService>();
        var okResponse = ControllerResponseBuilder.Success();
        var id = _fixture.Create<int>();
        mock.Setup(s => s.DeletePermissionAsync(id)).ReturnsAsync(okResponse);
        var controller = new PermissionController(mock.Object);

        // Act
        var result = await controller.DeletePermission(id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        mock.Verify(s => s.DeletePermissionAsync(id), Times.Once);
    }
}
