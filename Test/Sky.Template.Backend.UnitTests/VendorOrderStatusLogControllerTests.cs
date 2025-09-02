using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Contract.Requests.OrderStatusLogs;
using Sky.Template.Backend.Contract.Responses.OrderStatusLogResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.Vendor;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class VendorOrderStatusLogControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var service = new Mock<IVendorOrderStatusLogService>();
        var request = _fixture.Create<CreateOrderStatusLogRequest>();
        var response = ControllerResponseBuilder.Success(new OrderStatusLogResponse(), "Created", HttpStatusCode.Created);
        service.Setup(s => s.CreateAsync(request)).ReturnsAsync(response);
        var controller = new OrderStatusLogController(service.Object);

        var result = await controller.Create(request);

        result.Should().BeOfType<CreatedResult>();
        service.Verify(s => s.CreateAsync(request), Times.Once);
    }

    [Fact]
    public async Task Get_ReturnsOk()
    {
        var service = new Mock<IVendorOrderStatusLogService>();
        var response = ControllerResponseBuilder.Success(new OrderStatusLogListResponse());
        service.Setup(s => s.GetByOrderIdAsync(It.IsAny<Guid>())).ReturnsAsync(response);
        var controller = new OrderStatusLogController(service.Object);

        var result = await controller.Get(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
        service.Verify(s => s.GetByOrderIdAsync(It.IsAny<Guid>()), Times.Once);
    }
}
