using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Kyc;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.Kyc;
using Sky.Template.Backend.WebAPI.Controllers.User;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Net;
using Xunit;
using System.IO;

namespace Sky.Template.Backend.UnitTests;

public class UserKycControllerTests
{
    private readonly Fixture _fixture;

    public UserKycControllerTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });
    }

    [Fact]
    public async Task Start_ReturnsOk()
    {
        var serviceMock = _fixture.Create<Mock<IUserKycService>>();
        var request = _fixture.Create<KycVerificationRequest>();
        var ok = ControllerResponseBuilder.Success(new KycStatusResponse());
        serviceMock.Setup(s => s.StartVerificationAsync(request)).ReturnsAsync(ok);
        var controller = new KycController(serviceMock.Object);

        var result = await controller.Start(request);

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.StartVerificationAsync(request), Times.Once);
    }

    [Fact]
    public async Task Start_ReturnsForbid_WhenServiceReturnsForbidden()
    {
        var serviceMock = _fixture.Create<Mock<IUserKycService>>();
        var forbidden = ControllerResponseBuilder.Failure<KycStatusResponse>("", HttpStatusCode.Forbidden);
        serviceMock.Setup(s => s.StartVerificationAsync(It.IsAny<KycVerificationRequest>())).ReturnsAsync(forbidden);
        var controller = new KycController(serviceMock.Object);

        var result = await controller.Start(new KycVerificationRequest());

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task ResubmitKyc_ReturnsNoContent()
    {
        var serviceMock = _fixture.Create<Mock<IUserKycService>>();
        var controller = new KycController(serviceMock.Object);
        var userId = Guid.NewGuid();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }))
            }
        };
        var request = BuildSubmissionRequest();

        var result = await controller.ResubmitKyc(request);

        result.Should().BeOfType<NoContentResult>();
        serviceMock.Verify(s => s.ResubmitKycAsync(userId, request), Times.Once);
    }

    [Fact]
    public async Task DeleteKyc_ReturnsNoContent()
    {
        var serviceMock = _fixture.Create<Mock<IUserKycService>>();
        var controller = new KycController(serviceMock.Object);
        var userId = Guid.NewGuid();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }))
            }
        };
        var kycId = Guid.NewGuid();

        var result = await controller.DeleteKyc(kycId);

        result.Should().BeOfType<NoContentResult>();
        serviceMock.Verify(s => s.DeleteKycAsync(userId, kycId), Times.Once);
    }

    [Fact]
    public async Task Status_ReturnsOk()
    {
        var serviceMock = _fixture.Create<Mock<IUserKycService>>();
        var userId = Guid.NewGuid();
        var ok = ControllerResponseBuilder.Success(new KycStatusResponse());
        serviceMock.Setup(s => s.GetStatusAsync(userId)).ReturnsAsync(ok);
        var controller = new KycController(serviceMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }))
            }
        };

        var result = await controller.Status();

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.GetStatusAsync(userId), Times.Once);
    }
    private KycSubmissionRequest BuildSubmissionRequest()
    {
        var selfie = CreateFormFile("selfie.jpg");
        var front = CreateFormFile("front.jpg");
        var back = CreateFormFile("back.jpg");

        return _fixture.Build<KycSubmissionRequest>()
            .With(x => x.Selfie, selfie)
            .With(x => x.DocumentFront, front)
            .With(x => x.DocumentBack, back)
            .Create();
    }

    private static IFormFile CreateFormFile(string fileName)
    {
        var bytes = new byte[] { 1, 2, 3 };
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, fileName, fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }
}

