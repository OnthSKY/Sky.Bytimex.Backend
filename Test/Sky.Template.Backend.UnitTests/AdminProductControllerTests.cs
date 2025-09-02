using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminProductControllerTests
{
    [Fact]
    public async Task HardDeleteProduct_ReturnsOk()
    {
        var serviceMock = new Mock<IAdminProductService>();
        serviceMock.Setup(s => s.HardDeleteProductAsync(It.IsAny<Guid>())).ReturnsAsync(ControllerResponseBuilder.Success());
        var controller = new ProductController(serviceMock.Object);

        var result = await controller.HardDeleteProduct(Guid.NewGuid());

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        serviceMock.Verify(s => s.HardDeleteProductAsync(It.IsAny<Guid>()), Times.Once);
    }
}
