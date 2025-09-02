using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.PaymentMethods;
using Sky.Template.Backend.Contract.Responses.PaymentMethodResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminPaymentMethodControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task GetPaymentMethods_ReturnsOk()
    {
        var serviceMock = new Mock<IAdminPaymentMethodService>();
        serviceMock.Setup(s => s.GetFilteredPaginatedAsync(It.IsAny<PaymentMethodFilterRequest>()))
            .ReturnsAsync(ControllerResponseBuilder.Success(new PaymentMethodListPaginatedResponse
            {
                PaymentMethods = new PaginatedData<PaymentMethodResponse>
                {
                    Items = new List<PaymentMethodResponse>(),
                    TotalCount = 0,
                    Page = 1,
                    PageSize = 10,
                    TotalPage = 0
                }
            }));
        var controller = new PaymentMethodController(serviceMock.Object);

        var result = await controller.GetPaymentMethods(new PaymentMethodFilterRequest());

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.GetFilteredPaginatedAsync(It.IsAny<PaymentMethodFilterRequest>()), Times.Once);
    }

    [Fact]
    public async Task CreatePaymentMethod_ReturnsCreated()
    {
        var serviceMock = new Mock<IAdminPaymentMethodService>();
        var request = _fixture.Create<CreatePaymentMethodRequest>();
        serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(ControllerResponseBuilder.Success(new PaymentMethodResponse(), "Created", HttpStatusCode.Created));
        var controller = new PaymentMethodController(serviceMock.Object);

        var result = await controller.CreatePaymentMethod(request);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.StatusCode.Should().Be((int)HttpStatusCode.Created);
        serviceMock.Verify(s => s.CreateAsync(request), Times.Once);
    }

    [Fact]
    public async Task DeletePaymentMethod_ReturnsOk()
    {
        var serviceMock = new Mock<IAdminPaymentMethodService>();
        serviceMock.Setup(s => s.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(ControllerResponseBuilder.Success());
        var controller = new PaymentMethodController(serviceMock.Object);

        var result = await controller.DeletePaymentMethod(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.DeleteAsync(It.IsAny<Guid>()), Times.Once);
    }
}
