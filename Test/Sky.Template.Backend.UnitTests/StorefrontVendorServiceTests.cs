using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Sky.Template.Backend.Application.Services.Storefront;
using Sky.Template.Backend.Contract.Responses.Storefront;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Infrastructure.Entities.Vendor;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class StorefrontVendorServiceTests
{
    [Fact]
    public async Task GetVendorDetail_BySlug_ReturnsMapped()
    {
        var repo = new Mock<IStorefrontVendorRepository>();
        var entity = new VendorEntity
        {
            Id = Guid.NewGuid(),
            Name = "Acme",
            Slug = "acme",
            ShortDescription = "short",
            LogoUrl = "logo",
            BannerUrl = "banner",
            RatingAvg = 4.5m,
            RatingCount = 10,
            CreatedAt = new DateTime(2024,1,1)
        };
        repo.Setup(r => r.GetActiveBySlugAsync("acme")).ReturnsAsync(entity);
        var service = new StorefrontVendorService(repo.Object);
        var response = await service.GetVendorDetailAsync("acme", null);
        response.Data!.Slug.Should().Be("acme");
        response.Data.RatingCount.Should().Be(10);
    }

    [Fact]
    public async Task GetVendorDetail_NotFound_Throws()
    {
        var repo = new Mock<IStorefrontVendorRepository>();
        repo.Setup(r => r.GetActiveBySlugAsync("missing")).ReturnsAsync((VendorEntity?)null);
        var service = new StorefrontVendorService(repo.Object);
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetVendorDetailAsync("missing", null));
    }
}
