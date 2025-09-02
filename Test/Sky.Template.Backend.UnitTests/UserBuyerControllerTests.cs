using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.SaleResponses;
using Sky.Template.Backend.WebAPI.Controllers.User;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class UserBuyerControllerTests
{
    [Fact]
    public async Task GetBuyerById_ReturnsOk()
    {
        var service = new Mock<IUserBuyerService>();
        service.Setup(s => s.GetBuyerByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ControllerResponseBuilder.Success(new SingleBuyerResponse()));
        var controller = new BuyerController(service.Object);

        var result = await controller.GetBuyerById(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
        service.Verify(s => s.GetBuyerByIdAsync(It.IsAny<Guid>()), Times.Once);
    }
}
