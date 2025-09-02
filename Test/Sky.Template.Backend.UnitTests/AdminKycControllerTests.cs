using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Kyc;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.Kyc;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using System.Net;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminKycControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task Approve_ReturnsOk_WhenApproved()
    {
        var serviceMock = new Mock<IAdminKycService>();
        var request = _fixture.Create<KycApprovalRequest>();
        var ok = ControllerResponseBuilder.Success(new KycStatusResponse());
        serviceMock.Setup(s => s.ApproveVerificationAsync(request)).ReturnsAsync(ok);
        var controller = new KycController(serviceMock.Object);

        var result = await controller.Approve(request);

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.ApproveVerificationAsync(request), Times.Once);
    }

    [Fact]
    public async Task Approve_ReturnsForbidden_WhenServiceReturnsForbidden()
    {
        var serviceMock = new Mock<IAdminKycService>();
        var forbidden = ControllerResponseBuilder.Failure<KycStatusResponse>("", HttpStatusCode.Forbidden);
        serviceMock.Setup(s => s.ApproveVerificationAsync(It.IsAny<KycApprovalRequest>())).ReturnsAsync(forbidden);
        var controller = new KycController(serviceMock.Object);

        var result = await controller.Approve(new KycApprovalRequest());

        result.Should().BeOfType<ForbidResult>();
    }
}

