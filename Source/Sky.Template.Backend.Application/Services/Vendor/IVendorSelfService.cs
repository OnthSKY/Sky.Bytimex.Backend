using Sky.Template.Backend.Core.Constants;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.Vendors;
using Sky.Template.Backend.Contract.Responses.VendorResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Entities.Vendor;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.Vendor;

public interface IVendorSelfService
{
    Task<BaseControllerResponse<SingleVendorResponse>> GetVendorByIdAsync(Guid id);
    Task<BaseControllerResponse<SingleVendorResponse>> UpdateVendorAsync(UpdateVendorRequest request);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null);
    Task<BaseControllerResponse<VerificationStatusDto>> GetVerificationStatusAsync();
}

public class VendorSelfService : IVendorSelfService
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public VendorSelfService(IVendorRepository vendorRepository, IHttpContextAccessor httpContextAccessor)
    {
        _vendorRepository = vendorRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    [HasPermission(Permissions.Vendors.UpdateSelf)]
    public async Task<BaseControllerResponse<SingleVendorResponse>> GetVendorByIdAsync(Guid id)
    {
        var vendor = await _vendorRepository.GetByIdAsync(id);
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        if (vendor == null || vendor.Id != userId)
            throw new ForbiddenException("Auth.PermissionDenied");

        return ControllerResponseBuilder.Success(new SingleVendorResponse
        {
            Vendor = MapVendorToDto(vendor)
        });
    }

    [HasPermission(Permissions.Vendors.UpdateSelf)]
    public async Task<BaseControllerResponse<SingleVendorResponse>> UpdateVendorAsync(UpdateVendorRequest request)
    {
        var vendor = await _vendorRepository.GetByIdAsync(request.Id);
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        if (vendor == null || vendor.Id != userId)
            throw new ForbiddenException("Auth.PermissionDenied");

        vendor.Name = request.Name;
        vendor.Email = request.Email;
        vendor.Phone = request.Phone;
        vendor.Address = request.Address;
        vendor.Status = request.Status;
        vendor.UpdatedAt = DateTime.UtcNow;
        vendor.UpdatedBy = userId;

        await _vendorRepository.UpdateAsync(vendor);

        return ControllerResponseBuilder.Success(new SingleVendorResponse
        {
            Vendor = MapVendorToDto(vendor)
        }, "VendorUpdatedSuccessfully");
    }

    [HasPermission(Permissions.Vendors.UpdateSelf)]
    public async Task<BaseControllerResponse<VerificationStatusDto>> GetVerificationStatusAsync()
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var vendor = await _vendorRepository.GetByIdAsync(userId);
        if (vendor == null)
            throw new NotFoundException("VendorNotFound", userId);

        var dto = new VerificationStatusDto
        {
            Status = vendor.VerificationStatus,
            Note = vendor.VerificationNote,
            VerifiedAt = vendor.VerifiedAt
        };

        return ControllerResponseBuilder.Success(dto);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null)
        => await _vendorRepository.IsEmailUniqueAsync(email, excludeId);

    private static VendorDto MapVendorToDto(VendorEntity v) => new()
    {
        Id = v.Id,
        Name = v.Name,
        Email = v.Email,
        Phone = v.Phone,
        Address = v.Address,
        Status = v.Status,
        VerificationStatus = v.VerificationStatus,
        VerificationNote = v.VerificationNote,
        VerifiedAt = v.VerifiedAt,
        CreatedAt = v.CreatedAt,
        CreatedBy = v.CreatedBy,
        UpdatedAt = v.UpdatedAt,
        UpdatedBy = v.UpdatedBy,
        IsDeleted = v.IsDeleted,
        DeletedAt = v.DeletedAt,
        DeletedBy = v.DeletedBy,
        DeleteReason = v.DeleteReason
    };
}

