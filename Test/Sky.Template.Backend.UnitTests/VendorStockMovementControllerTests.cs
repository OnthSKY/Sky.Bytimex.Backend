using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Contract.Requests.StockMovements;
using Sky.Template.Backend.Contract.Responses.StockMovementResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.Vendor;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class VendorStockMovementControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var service = new Mock<IVendorStockMovementService>();
        var request = _fixture.Create<CreateStockMovementRequest>();
        var response = ControllerResponseBuilder.Success(new StockMovementResponse(), "Created", HttpStatusCode.Created);
        service.Setup(s => s.CreateAsync(request)).ReturnsAsync(response);
        var controller = new StockMovementController(service.Object);

        var result = await controller.Create(request);

        result.Should().BeOfType<CreatedResult>();
        service.Verify(s => s.CreateAsync(request), Times.Once);
    }

    [Fact]
    public async Task Get_ReturnsOk()
    {
        var service = new Mock<IVendorStockMovementService>();
        var resp = ControllerResponseBuilder.Success(new StockMovementListResponse());
        service.Setup(s => s.GetAsync(It.IsAny<StockMovementFilterRequest>())).ReturnsAsync(resp);
        var controller = new StockMovementController(service.Object);

        var result = await controller.Get(new StockMovementFilterRequest());

        result.Should().BeOfType<OkObjectResult>();
        service.Verify(s => s.GetAsync(It.IsAny<StockMovementFilterRequest>()), Times.Once);
    }
}
