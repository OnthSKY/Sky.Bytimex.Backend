using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class UserBuyerServiceTests
{
    [Fact]
    public async Task GetBuyerByIdAsync_NotFoundThrows()
    {
        // Arrange
        var repo = new Mock<IBuyerRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((BuyerEntity?)null);

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        
        var service = new UserBuyerService(repo.Object, httpContextAccessor.Object);

        // Act
        Func<Task> act = () => service.GetBuyerByIdAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }


}
