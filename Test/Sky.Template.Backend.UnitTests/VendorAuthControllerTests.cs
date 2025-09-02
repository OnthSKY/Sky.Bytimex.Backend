using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Contract.Requests.Auth;
using Sky.Template.Backend.Contract.Responses.Auth;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.Vendor;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class VendorAuthControllerTests
{
    [Fact]
    public async Task RegisterVendor_ReturnsOk_WhenSuccessful()
    {
        var mockService = new Mock<IVendorAuthService>();
        var response = ControllerResponseBuilder.Success(new AuthResponse());
        mockService.Setup(s => s.RegisterVendorAsync(It.IsAny<RegisterVendorRequest>())).ReturnsAsync(response);
        var controller = new VendorAuthController(mockService.Object);

        var result = await controller.RegisterVendor(new RegisterVendorRequest
        {
            Email = "a@b.com",
            Phone = "+10000000000",
            Password = "Passw0rd!",
            FirstName = "A",
            LastName = "B"
        });

        result.Should().BeOfType<OkObjectResult>();
        mockService.Verify(s => s.RegisterVendorAsync(It.IsAny<RegisterVendorRequest>()), Times.Once);
    }
}
