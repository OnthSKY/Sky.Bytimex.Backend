using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.PaymentMethods;
using Sky.Template.Backend.Infrastructure.Entities.System;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminPaymentMethodServiceTests
{
    private readonly Fixture _fixture = new();

    private AdminPaymentMethodService CreateService(Mock<IPaymentMethodRepository> repoMock, Mock<IPaymentMethodTranslationRepository>? trRepoMock = null)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(context);
        return new AdminPaymentMethodService(repoMock.Object, (trRepoMock ?? new Mock<IPaymentMethodTranslationRepository>()).Object, accessor.Object);
    }

    [Fact]
    public async Task CreateAsync_CallsRepositoryCreate()
    {
        var repoMock = new Mock<IPaymentMethodRepository>();
        repoMock.Setup(r => r.IsCodeUniqueAsync(It.IsAny<string>(), null)).ReturnsAsync(true);
        repoMock.Setup(r => r.CreateAsync(It.IsAny<PaymentMethodEntity>())).ReturnsAsync(new PaymentMethodEntity());
        var trRepoMock = new Mock<IPaymentMethodTranslationRepository>();
        trRepoMock.Setup(t => t.UpsertAsync(It.IsAny<PaymentMethodTranslationEntity>(), null, null)).ReturnsAsync(new PaymentMethodTranslationEntity());
        var service = CreateService(repoMock, trRepoMock);
        var request = _fixture.Create<CreatePaymentMethodRequest>();

        await service.CreateAsync(request);

        repoMock.Verify(r => r.CreateAsync(It.IsAny<PaymentMethodEntity>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPaymentMethod()
    {
        var repoMock = new Mock<IPaymentMethodRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new PaymentMethodEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Code = "TEST",
            SupportedCurrency = "USD",
            Type = "CARD",
            Status = "ACTIVE",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        var trRepoMock = new Mock<IPaymentMethodTranslationRepository>();
        trRepoMock.Setup(t => t.GetAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync((PaymentMethodTranslationEntity?)null);
        var service = CreateService(repoMock, trRepoMock);

        var response = await service.GetByIdAsync(Guid.NewGuid());

        response.Data.Should().NotBeNull();
        repoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }
}
