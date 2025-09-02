using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.OrderDetails;
using Sky.Template.Backend.Contract.Responses.OrderDetailResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminOrderDetailControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var serviceMock = new Mock<IAdminOrderDetailService>();
        var request = _fixture.Create<CreateOrderDetailRequest>();
        var created = ControllerResponseBuilder.Success(new OrderDetailResponse(), "Created", HttpStatusCode.Created);
        serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(created);
        var controller = new OrderDetailController(serviceMock.Object);

        var result = await controller.Create(request);

        result.Should().BeOfType<CreatedResult>();
        serviceMock.Verify(s => s.CreateAsync(request), Times.Once);
    }

    [Fact]
    public async Task GetByOrderId_ReturnsOk()
    {
        var serviceMock = new Mock<IAdminOrderDetailService>();
        var response = ControllerResponseBuilder.Success(new OrderDetailListResponse());
        serviceMock.Setup(s => s.GetByOrderIdAsync(It.IsAny<Guid>())).ReturnsAsync(response);
        var controller = new OrderDetailController(serviceMock.Object);

        var result = await controller.GetByOrderId(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.GetByOrderIdAsync(It.IsAny<Guid>()), Times.Once);
    }
}

