using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Payments;
using Sky.Template.Backend.Contract.Responses.PaymentResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminPaymentControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task GetPayment_ReturnsOk()
    {
        var serviceMock = new Mock<IAdminPaymentService>();
        serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ControllerResponseBuilder.Success(new PaymentResponse()));
        var controller = new PaymentController(serviceMock.Object);

        var result = await controller.GetPayment(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task CreatePayment_ReturnsCreated()
    {
        var serviceMock = new Mock<IAdminPaymentService>();
        var request = _fixture.Create<CreatePaymentRequest>();
        serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(ControllerResponseBuilder.Success(new PaymentResponse(), "Created", HttpStatusCode.Created));
        var controller = new PaymentController(serviceMock.Object);

        var result = await controller.CreatePayment(request);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.StatusCode.Should().Be((int)HttpStatusCode.Created);
        serviceMock.Verify(s => s.CreateAsync(request), Times.Once);
    }

    [Fact]
    public async Task DeletePayment_ReturnsOk()
    {
        var serviceMock = new Mock<IAdminPaymentService>();
        serviceMock.Setup(s => s.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(ControllerResponseBuilder.Success());
        var controller = new PaymentController(serviceMock.Object);

        var result = await controller.DeletePayment(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.DeleteAsync(It.IsAny<Guid>()), Times.Once);
    }
}
