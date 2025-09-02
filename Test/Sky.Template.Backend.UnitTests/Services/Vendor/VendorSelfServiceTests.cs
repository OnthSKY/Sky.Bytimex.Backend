using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Contract.Requests.Vendors;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Enums;
using Sky.Template.Backend.Infrastructure.Entities.Vendor;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests.Services.Vendor;

public class VendorSelfServiceTests
{
    private VendorSelfService CreateService(Mock<IVendorRepository> repoMock, Guid? userId = null)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        var uid = userId ?? Guid.NewGuid();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, uid.ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(context);
        return new VendorSelfService(repoMock.Object, accessor.Object);
    }

    [Fact]
    public async Task UpdateVendorAsync_Throws_ForDifferentUser()
    {
        var repo = new Mock<IVendorRepository>();
        var entity = new VendorEntity { Id = Guid.NewGuid(), Name = "Old" };
        repo.Setup(r => r.GetByIdAsync(entity.Id)).ReturnsAsync(entity);
        var service = CreateService(repo, Guid.NewGuid());

        var request = new UpdateVendorRequest { Id = entity.Id, Name = "New", Status = "ACTIVE" };

        Func<Task> act = () => service.UpdateVendorAsync(request);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task GetVerificationStatusAsync_ReturnsDto()
    {
        var repo = new Mock<IVendorRepository>();
        var vendorId = Guid.NewGuid();
        var entity = new VendorEntity { Id = vendorId, VerificationStatus = VerificationStatus.PENDING.ToString(), VerificationNote = "note" };
        repo.Setup(r => r.GetByIdAsync(vendorId)).ReturnsAsync(entity);
        var service = CreateService(repo, vendorId);

        var response = await service.GetVerificationStatusAsync();

        response.Data.Should().NotBeNull();
        response.Data!.Status.Should().Be(VerificationStatus.PENDING.ToString());
        response.Data.Note.Should().Be("note");
    }
}

