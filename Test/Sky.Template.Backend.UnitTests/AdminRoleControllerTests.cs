using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Roles;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.RoleResponses;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminRoleControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task CreateRole_ReturnsCreated_WhenValid()
    {
        var mock = new Mock<IAdminRoleService>();
        var request = _fixture.Create<CreateRoleRequest>();
        var response = ControllerResponseBuilder.Success(new SingleRoleResponse(), "Created", HttpStatusCode.Created);
        mock.Setup(s => s.CreateRoleAsync(request)).ReturnsAsync(response);
        var controller = new RoleController(mock.Object);

        var result = await controller.CreateRole(request);

        result.Should().BeOfType<CreatedResult>();
        mock.Verify(s => s.CreateRoleAsync(request), Times.Once);
    }

    [Fact]
    public async Task DeleteRole_ReturnsNotFound_WhenServiceReturnsNotFound()
    {
        var mock = new Mock<IAdminRoleService>();
        var notFound = ControllerResponseBuilder.Failure("NotFound", HttpStatusCode.NotFound);
        mock.Setup(s => s.DeleteRoleAsync(It.IsAny<int>())).ReturnsAsync(notFound);
        var controller = new RoleController(mock.Object);

        var result = await controller.DeleteRole(1);

        result.Should().BeOfType<NotFoundObjectResult>();
        mock.Verify(s => s.DeleteRoleAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task AddPermissionToRole_ReturnsOk_WhenSuccessful()
    {
        var mock = new Mock<IAdminRoleService>();
        var ok = ControllerResponseBuilder.Success();
        mock.Setup(s => s.AddPermissionToRoleAsync(It.Is<AddPermissionToRoleRequest>(r => r.RoleId == 1 && r.PermissionId == 2)))
            .ReturnsAsync(ok);
        var controller = new RoleController(mock.Object);

        var result = await controller.AddPermissionToRole(1, 2);

        result.Should().BeOfType<OkObjectResult>();
        mock.Verify(s => s.AddPermissionToRoleAsync(It.IsAny<AddPermissionToRoleRequest>()), Times.Once);
    }
}
