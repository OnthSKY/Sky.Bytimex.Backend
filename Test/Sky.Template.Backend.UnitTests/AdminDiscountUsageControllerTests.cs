using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.DiscountUsages;
using Sky.Template.Backend.Contract.Responses.DiscountUsageResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminDiscountUsageControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task CreateUsage_ReturnsCreated()
    {
        var serviceMock = new Mock<IAdminDiscountUsageService>();
        var request = _fixture.Create<CreateDiscountUsageRequest>();
        var response = ControllerResponseBuilder.Success(new DiscountUsageResponse(), "Created", HttpStatusCode.Created);
        serviceMock.Setup(s => s.CreateDiscountUsageAsync(request)).ReturnsAsync(response);
        var controller = new DiscountUsageController(serviceMock.Object);

        var result = await controller.CreateUsage(request);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.StatusCode.Should().Be((int)HttpStatusCode.Created);
    }
}
