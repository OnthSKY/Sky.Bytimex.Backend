using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Orders;
using Sky.Template.Backend.Contract.Responses.SaleResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminOrderControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task CreateOrder_WhenRequestIsValid_ShouldReturnCreated()
    {
        var serviceMock = new Mock<IAdminOrderService>();
        var request = _fixture.Create<CreateOrderRequest>();
        var created = ControllerResponseBuilder.Success(new SingleOrderResponse(), "Created", HttpStatusCode.Created);
        serviceMock.Setup(s => s.CreateSaleAsync(request)).ReturnsAsync(created);
        var controller = new OrderController(serviceMock.Object);

        var result = await controller.CreateSale(request);

        result.Should().BeOfType<CreatedResult>();
        serviceMock.Verify(s => s.CreateSaleAsync(request), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteOrder_WhenDeletionSucceeds_ShouldReturnOk()
    {
        var serviceMock = new Mock<IAdminOrderService>();
        var response = ControllerResponseBuilder.Success();
        serviceMock.Setup(s => s.SoftDeleteSaleAsync(It.IsAny<Guid>())).ReturnsAsync(response);
        var controller = new OrderController(serviceMock.Object);

        var result = await controller.SoftDeleteSale(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.SoftDeleteSaleAsync(It.IsAny<Guid>()), Times.Once);
    }
}
