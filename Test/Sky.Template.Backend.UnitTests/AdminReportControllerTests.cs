using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.ReportResponses;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminReportControllerTests
{
    [Fact]
    public async Task GetVendorCount_ReturnsOk()
    {
        var serviceMock = new Mock<IAdminReportService>();
        var response = ControllerResponseBuilder.Success(new VendorCountResponse { Count = 3 });
        serviceMock.Setup(s => s.GetVendorCountAsync()).ReturnsAsync(response);
        var controller = new ReportController(serviceMock.Object);

        var result = await controller.GetVendorCount();

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.GetVendorCountAsync(), Times.Once);
    }

    [Fact]
    public async Task GetVendorSales_ReturnsOk()
    {
        var serviceMock = new Mock<IAdminReportService>();
        var response = ControllerResponseBuilder.Success(new VendorSalesReportListResponse());
        serviceMock.Setup(s => s.GetSalesByVendorAsync(null, null)).ReturnsAsync(response);
        var controller = new ReportController(serviceMock.Object);

        var result = await controller.GetVendorSales(null, null);

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.GetSalesByVendorAsync(null, null), Times.Once);
    }
}
