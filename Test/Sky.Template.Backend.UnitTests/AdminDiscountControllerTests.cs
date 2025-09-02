using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Discounts;
using Sky.Template.Backend.Contract.Responses.DiscountResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminDiscountControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task GetDiscountById_ReturnsOk()
    {
        var serviceMock = new Mock<IAdminDiscountService>();
        var response = ControllerResponseBuilder.Success(new DiscountResponse());
        serviceMock.Setup(s => s.GetDiscountByIdAsync(It.IsAny<Guid>())).ReturnsAsync(response);
        var controller = new DiscountController(serviceMock.Object);

        var result = await controller.GetDiscountById(Guid.NewGuid());

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateDiscount_ReturnsCreated()
    {
        var serviceMock = new Mock<IAdminDiscountService>();
        var request = _fixture.Create<CreateDiscountRequest>();
        var response = ControllerResponseBuilder.Success(new DiscountResponse(), "Created", HttpStatusCode.Created);
        serviceMock.Setup(s => s.CreateDiscountAsync(request)).ReturnsAsync(response);
        var controller = new DiscountController(serviceMock.Object);

        var result = await controller.CreateDiscount(request);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.StatusCode.Should().Be((int)HttpStatusCode.Created);
    }
}
