using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Users;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.UserResponses;
using Sky.Template.Backend.WebAPI.Controllers.User;
using Xunit;

namespace Sky.Template.Backend.UnitTests.Controllers.User;

public class SelfUserControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task UpdateProfile_ReturnsOk()
    {
        // Arrange
        var serviceMock = new Mock<IUserService>();
        var req = _fixture.Create<SelfUpdateProfileRequest>();

        var expectedUserId = Guid.NewGuid();
        var response = ControllerResponseBuilder.Success(new SingleUserResponse());
        serviceMock
            .Setup(s => s.UpdateSelfProfileAsync(expectedUserId, It.IsAny<SelfUpdateProfileRequest>()))
            .ReturnsAsync(response);

        var controller = new SelfUserController(serviceMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString())
        }, "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await controller.UpdateProfile(req);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.UpdateSelfProfileAsync(expectedUserId, It.Is<SelfUpdateProfileRequest>(r =>
            r.FirstName == req.FirstName && r.LastName == req.LastName)), Times.Once);
    }
}
