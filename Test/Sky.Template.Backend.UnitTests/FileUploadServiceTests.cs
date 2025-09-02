using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.System;
using Sky.Template.Backend.Contract.Requests.FileUploads;
using Sky.Template.Backend.Contract.Responses.FileUploadResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Infrastructure.Entities.System;
using Sky.Template.Backend.Infrastructure.Repositories.System;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class FileUploadServiceTests
{
    private readonly Fixture _fixture = new();

    private FileUploadService CreateService(Mock<IFileUploadRepository> repoMock)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        var userId = Guid.NewGuid();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]{ new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(context);
        return new FileUploadService(repoMock.Object, accessor.Object);
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedResponse()
    {
        var repoMock = new Mock<IFileUploadRepository>();
        var request = _fixture.Create<CreateFileUploadRequest>();
        var entity = new FileUploadEntity
        {
            Id = Guid.NewGuid(),
            FileName = request.FileName,
            FileExtension = request.FileExtension,
            FileSize = request.FileSize,
            FileUrl = request.FileUrl,
            FileType = request.FileType,
            Context = request.Context,
            UploadedBy = Guid.NewGuid(),
            UploadedAt = DateTime.UtcNow,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow
        };
        repoMock.Setup(r => r.CreateAsync(It.IsAny<FileUploadEntity>())).ReturnsAsync(entity);
        var service = CreateService(repoMock);

        var response = await service.CreateAsync(request);

        response.Data.Should().NotBeNull();
        response.Data!.FileName.Should().Be(request.FileName);
        repoMock.Verify(r => r.CreateAsync(It.IsAny<FileUploadEntity>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsSuccess()
    {
        var repoMock = new Mock<IFileUploadRepository>();
        repoMock.Setup(r => r.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(true);
        var service = CreateService(repoMock);

        var result = await service.DeleteAsync(Guid.NewGuid());

        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        repoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Once);
    }
}

