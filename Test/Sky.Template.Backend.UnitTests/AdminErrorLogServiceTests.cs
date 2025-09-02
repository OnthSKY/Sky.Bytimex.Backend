using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.ErrorLogs;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Infrastructure.Entities.ErrorLog;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminErrorLogServiceTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task LogErrorAsync_InsertsLog()
    {
        var repoMock = new Mock<IErrorLogRepository>();
        var uowMock = new Mock<IUnitOfWork>();
        var service = new AdminErrorLogService(repoMock.Object, uowMock.Object);
        var request = _fixture.Create<CreateErrorLogRequest>();

        await service.LogErrorAsync(request);

        repoMock.Verify(r => r.InsertAsync(It.IsAny<ErrorLogEntity>()), Times.Once);
     }

    [Fact]
    public async Task GetAllAsync_ReturnsLogs()
    {
        var repoMock = new Mock<IErrorLogRepository>();
        var uowMock = new Mock<IUnitOfWork>();
        var logs = _fixture.CreateMany<ErrorLogEntity>(3);
        repoMock.Setup(r => r.GetAllAsync(It.IsAny<GridRequest>()))
            .ReturnsAsync((logs, logs.Count()));
        var service = new AdminErrorLogService(repoMock.Object, uowMock.Object);
        var request = new GridRequest { Page = 1, PageSize = 10 };

        var response = await service.GetAllAsync(request);

        response.Data.Should().NotBeNull();
        response.Data!.Logs.Items.Count.Should().Be(3);
        repoMock.Verify(r => r.GetAllAsync(It.IsAny<GridRequest>()), Times.Once);
    }
}
