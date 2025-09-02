using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Vendors;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Enums;
using Sky.Template.Backend.Infrastructure.Entities.Vendor;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;
using Xunit;

namespace Sky.Template.Backend.UnitTests.Services.Vendor;

public class AdminVendorServiceTests
{
    private AdminVendorService CreateService(Mock<IVendorRepository> repoMock)
    {
        var uow = new Mock<IUnitOfWork>();
        var accessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]{ new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(context);
        return new AdminVendorService(repoMock.Object, uow.Object, accessor.Object);
    }

    [Fact]
    public async Task CreateVendorAsync_CallsRepository()
    {
        var repo = new Mock<IVendorRepository>();
        var service = CreateService(repo);
        var request = new CreateVendorRequest { Name = "Vendor", Status = "ACTIVE" };

        var result = await service.CreateVendorAsync(request);

        result.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        repo.Verify(r => r.CreateAsync(It.IsAny<VendorEntity>()), Times.Once);
    }

    [Fact]
    public async Task GetVendorByIdAsync_NotFound_Throws()
    {
        var repo = new Mock<IVendorRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((VendorEntity?)null);
        var service = CreateService(repo);

        Func<Task> act = () => service.GetVendorByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateVendorAsync_ReturnsUpdatedVendor()
    {
        var repo = new Mock<IVendorRepository>();
        var entity = new VendorEntity { Id = Guid.NewGuid(), Name = "Old" };
        repo.Setup(r => r.GetByIdAsync(entity.Id)).ReturnsAsync(entity);
        repo.Setup(r => r.UpdateAsync(It.IsAny<VendorEntity>()))
            .ReturnsAsync((VendorEntity input) => input);
        var service = CreateService(repo);
        var request = new UpdateVendorRequest { Id = entity.Id, Name = "New", Status = "ACTIVE" };

        var response = await service.UpdateVendorAsync(request);

        response.Data.Should().NotBeNull();
        repo.Verify(r => r.UpdateAsync(entity), Times.Once);
    }

    [Fact]
    public async Task ApproveVendorAsync_SetsStatusAndCallsUpdate()
    {
        var repo = new Mock<IVendorRepository>();
        var vendorId = Guid.NewGuid();
        var entity = new VendorEntity { Id = vendorId };
        repo.Setup(r => r.GetByIdAsync(vendorId)).ReturnsAsync(entity);
        var service = CreateService(repo);

        await service.ApproveVendorAsync(vendorId, "note");

        repo.Verify(r => r.UpdateAsync(It.Is<VendorEntity>(v => v.VerificationStatus == VerificationStatus.APPROVED.ToString() && v.VerificationNote == "note")), Times.Once);
    }

    [Fact]
    public async Task RejectVendorAsync_SetsStatusAndCallsUpdate()
    {
        var repo = new Mock<IVendorRepository>();
        var vendorId = Guid.NewGuid();
        var entity = new VendorEntity { Id = vendorId };
        repo.Setup(r => r.GetByIdAsync(vendorId)).ReturnsAsync(entity);
        var service = CreateService(repo);

        await service.RejectVendorAsync(vendorId, "note");

        repo.Verify(r => r.UpdateAsync(It.Is<VendorEntity>(v => v.VerificationStatus == VerificationStatus.REJECTED.ToString() && v.VerificationNote == "note")), Times.Once);
    }
}
