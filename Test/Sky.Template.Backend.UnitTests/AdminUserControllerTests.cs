using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.AdminUsers;
using Sky.Template.Backend.Contract.Requests.Roles;
using Sky.Template.Backend.Contract.Requests.Users;
using Sky.Template.Backend.Contract.Responses.RoleResponses;
using Sky.Template.Backend.Contract.Responses.UserResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Sky.Template.Backend.WebAPI.Controllers.User;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminUserControllerTests
{
    [Fact]
    public async Task GetAllUsers_ReturnsOk()
    {
        var mock = new Mock<IAdminUserService>();
        var response = ControllerResponseBuilder.Success(new FilteredUsersResponse());
        mock.Setup(s => s.GetAllUsersAsync(It.IsAny<UsersFilterRequest>())).ReturnsAsync(response);
        var controller = new AdminUserController(mock.Object);

        var result = await controller.GetAllUsers(new UsersFilterRequest());

        result.Should().BeOfType<OkObjectResult>();
        mock.Verify(s => s.GetAllUsersAsync(It.IsAny<UsersFilterRequest>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_ReturnsOk_WhenSuccessful()
    {
        var mock = new Mock<IAdminUserService>();
        var controllerRequest = new AdminUpdateUserRequest()
        {
            Id = Guid.NewGuid(),
            FirstName = "A",
            LastName = "B",
            Email = "a@b.c",
            Status = "ACTIVE"
        };
        var response = ControllerResponseBuilder.Success(new SingleUserResponse());

        mock.Setup(s => s.UpdateUserAsync(It.IsAny<UpdateUserRequest>())).ReturnsAsync(response);
        var controller = new AdminUserController(mock.Object);

        var result = await controller.UpdateUsers(controllerRequest);

        result.Should().BeOfType<OkObjectResult>();
        mock.Verify(s => s.UpdateUserAsync(It.Is<UpdateUserRequest>(r =>
            r.Id == controllerRequest.Id &&
            r.FirstName == controllerRequest.FirstName &&
            r.LastName == controllerRequest.LastName &&
            r.Email == controllerRequest.Email &&
            r.Status == controllerRequest.Status
        )), Times.Once);
    }


    [Fact]
    public async Task SoftDeleteUser_ReturnsOk_WhenSuccessful()
    {
        var mock = new Mock<IAdminUserService>();
        var ok = ControllerResponseBuilder.Success();
        mock.Setup(s => s.SoftDeleteUserAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(ok);
        var controller = new AdminUserController(mock.Object);

        var result = await controller.SoftDelete(Guid.NewGuid(), string.Empty);

        result.Should().BeOfType<OkObjectResult>();
        mock.Verify(s => s.SoftDeleteUserAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
    }

    //[Fact]
    //public async Task ChangeUserRole_ReturnsOk_WhenSuccessful()
    //{
    //    var mock = new Mock<IAdminUserService>();
    //    var request = new UpdateUserRoleRequest { UserId = Guid.NewGuid(), RoleId = 1 };
    //    var response = ControllerResponseBuilder.Success(new UpdateUserRoleResponse());
    //    mock.Setup(s => s.ChangeUserRoleAsync(request)).ReturnsAsync(response);
    //    var controller = new AdminUserController(mock.Object);

    //    var result = await controller.ChangeUserRole(request);

    //    result.Should().BeOfType<OkObjectResult>();
    //    mock.Verify(s => s.ChangeUserRoleAsync(request), Times.Once);
    //}
}
