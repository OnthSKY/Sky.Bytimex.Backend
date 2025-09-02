using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Contract.Requests.Invoices;
using Sky.Template.Backend.Contract.Responses.InvoiceResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.Vendor;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class VendorInvoiceControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task GetInvoice_ReturnsOk()
    {
        var serviceMock = new Mock<IVendorInvoiceService>();
        serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ControllerResponseBuilder.Success(new InvoiceResponse()));
        var controller = new InvoiceController(serviceMock.Object);

        var result = await controller.GetInvoice(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task CreateInvoice_ReturnsCreated()
    {
        var serviceMock = new Mock<IVendorInvoiceService>();
        var request = _fixture.Create<CreateInvoiceRequest>();
        serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(ControllerResponseBuilder.Success(new InvoiceResponse(), "Created", HttpStatusCode.Created));
        var controller = new InvoiceController(serviceMock.Object);

        var result = await controller.CreateInvoice(request);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.StatusCode.Should().Be((int)HttpStatusCode.Created);
        serviceMock.Verify(s => s.CreateAsync(request), Times.Once);
    }

    [Fact]
    public async Task DeleteInvoice_ReturnsOk()
    {
        var serviceMock = new Mock<IVendorInvoiceService>();
        serviceMock.Setup(s => s.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(ControllerResponseBuilder.Success());
        var controller = new InvoiceController(serviceMock.Object);

        var result = await controller.DeleteInvoice(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.DeleteAsync(It.IsAny<Guid>()), Times.Once);
}
}
