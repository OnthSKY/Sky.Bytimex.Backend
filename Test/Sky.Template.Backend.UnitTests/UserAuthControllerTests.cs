using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Auth;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.Auth;
using Sky.Template.Backend.WebAPI.Controllers.User;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class UserAuthControllerTests
{
    [Fact]
    public async Task Register_ReturnsOk_WhenSuccessful()
    {
        var mockService = new Mock<IUserAuthService>();
        var response = ControllerResponseBuilder.Success(new AuthResponse());
        mockService
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterUserRequest>()))
            .ReturnsAsync(response);
        var controller = new UserAuthController(mockService.Object);

        var result = await controller.Register(new RegisterUserRequest
        {
            Email = "a@b.com",
            Password = "Passw0rd!",
            Username = "ab",
            FirstName = "A",
            LastName = "B",
            Phone = "+10000000000"
        });

        result.Should().BeOfType<OkObjectResult>();
        mockService.Verify(s => s.RegisterAsync(It.IsAny<RegisterUserRequest>()), Times.Once);
    }
}
