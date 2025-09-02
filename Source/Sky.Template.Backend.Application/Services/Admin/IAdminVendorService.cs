using Sky.Template.Backend.Core.Constants;
using System.Net;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.Vendors;
using Sky.Template.Backend.Contract.Responses.VendorResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Core.Enums;
using Sky.Template.Backend.Infrastructure.Entities.Vendor;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminVendorService
{
    Task<BaseControllerResponse<PaginatedVendorListResponse>> GetFilteredPaginatedVendorsAsync(VendorFilterRequest request);
    Task<BaseControllerResponse<VendorListResponse>> GetAllVendorsAsync();
    Task<BaseControllerResponse<SingleVendorResponse>> GetVendorByIdAsync(Guid id);
    Task<BaseControllerResponse<SingleVendorResponse>> GetVendorByEmailAsync(string email);
    Task<BaseControllerResponse<SingleVendorResponse>> CreateVendorAsync(CreateVendorRequest request);
    Task<BaseControllerResponse<SingleVendorResponse>> UpdateVendorAsync(UpdateVendorRequest request);
    Task<BaseControllerResponse> DeleteVendorAsync(Guid id);
    Task<BaseControllerResponse> SoftDeleteVendorAsync(Guid id, string reason = "");
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null);
    Task<BaseControllerResponse> ApproveVendorAsync(Guid vendorId, string? note);
    Task<BaseControllerResponse> RejectVendorAsync(Guid vendorId, string? note);
}

 [HasPermission(Permissions.Vendors.All)]
public class AdminVendorService : IAdminVendorService
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdminVendorService(IVendorRepository vendorRepository, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _vendorRepository = vendorRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    [HasPermission(Permissions.Vendors.View)]
    public async Task<BaseControllerResponse<PaginatedVendorListResponse>> GetFilteredPaginatedVendorsAsync(VendorFilterRequest request)
    {
        var (vendors, totalCount) = await _vendorRepository.GetFilteredPaginatedAsync(request);

        if (!vendors.Any())
            throw new NotFoundException("VendorNotFound");

        var list = vendors.Select(MapVendorToDto).ToList();

        return ControllerResponseBuilder.Success(new PaginatedVendorListResponse
        {
            Vendors = new PaginatedData<VendorDto>
            {
                Items = list,
                TotalCount = totalCount,
                PageSize = vendors.Count(),
                Page = request.Page,
                TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
            }
        });
    }

    [HasPermission(Permissions.Vendors.View)]
    public async Task<BaseControllerResponse<VendorListResponse>> GetAllVendorsAsync()
    {
        var data = await _vendorRepository.GetAllAsync();
        if (!data.Any())
            throw new NotFoundException("VendorNotFound");

        return ControllerResponseBuilder.Success(new VendorListResponse
        {
            Vendors = data.Select(MapVendorToDto).ToList()
        });
    }

    [HasPermission(Permissions.Vendors.View)]
    public async Task<BaseControllerResponse<SingleVendorResponse>> GetVendorByIdAsync(Guid id)
    {
        var vendor = await _vendorRepository.GetByIdAsync(id);
        if (vendor == null)
            throw new NotFoundException("VendorNotFoundWithId", id);

        return ControllerResponseBuilder.Success(new SingleVendorResponse
        {
            Vendor = MapVendorToDto(vendor)
        });
    }

    [HasPermission(Permissions.Vendors.View)]
    public async Task<BaseControllerResponse<SingleVendorResponse>> GetVendorByEmailAsync(string email)
    {
        var vendor = await _vendorRepository.GetVendorByEmailAsync(email);
        if (vendor == null)
            throw new NotFoundException("VendorNotFoundWithEmail", email);

        return ControllerResponseBuilder.Success(new SingleVendorResponse
        {
            Vendor = MapVendorToDto(vendor)
        });
    }

    [HasPermission(Permissions.Vendors.Create)]
    public async Task<BaseControllerResponse<SingleVendorResponse>> CreateVendorAsync(CreateVendorRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var vendor = new VendorEntity
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            Status = request.Status,
            VerificationStatus = VerificationStatus.PENDING.ToString(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };

        await _vendorRepository.CreateAsync(vendor);

        return ControllerResponseBuilder.Success(new SingleVendorResponse
        {
            Vendor = MapVendorToDto(vendor)
        }, "VendorCreatedSuccessfully", HttpStatusCode.Created);
    }

    [HasPermission(Permissions.Vendors.Update)]
    public async Task<BaseControllerResponse<SingleVendorResponse>> UpdateVendorAsync(UpdateVendorRequest request)
    {
        var vendor = await _vendorRepository.GetByIdAsync(request.Id);
        if (vendor == null)
            throw new NotFoundException("VendorNotFound", request.Id);

        var userId = _httpContextAccessor.HttpContext.GetUserId();
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

    [HasPermission(Permissions.Vendors.Verify)]
    public async Task<BaseControllerResponse> ApproveVendorAsync(Guid vendorId, string? note)
    {
        var vendor = await _vendorRepository.GetByIdAsync(vendorId);
        if (vendor == null)
            throw new NotFoundException("VendorNotFound", vendorId);

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        vendor.VerificationStatus = VerificationStatus.APPROVED.ToString();
        vendor.VerificationNote = note;
        vendor.VerifiedAt = DateTime.UtcNow;
        vendor.UpdatedAt = DateTime.UtcNow;
        vendor.UpdatedBy = userId;

        await _vendorRepository.UpdateAsync(vendor);

        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.Vendors.Verify)]
    public async Task<BaseControllerResponse> RejectVendorAsync(Guid vendorId, string? note)
    {
        var vendor = await _vendorRepository.GetByIdAsync(vendorId);
        if (vendor == null)
            throw new NotFoundException("VendorNotFound", vendorId);

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        vendor.VerificationStatus = VerificationStatus.REJECTED.ToString();
        vendor.VerificationNote = note;
        vendor.VerifiedAt = DateTime.UtcNow;
        vendor.UpdatedAt = DateTime.UtcNow;
        vendor.UpdatedBy = userId;

        await _vendorRepository.UpdateAsync(vendor);

        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.Vendors.Delete)]
    public async Task<BaseControllerResponse> DeleteVendorAsync(Guid id)
    {
        var vendor = await _vendorRepository.GetByIdAsync(id);
        if (vendor == null)
            throw new NotFoundException("VendorNotFoundWithId", id);

        await _vendorRepository.DeleteAsync(id);
        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.Vendors.SoftDelete)]
    public async Task<BaseControllerResponse> SoftDeleteVendorAsync(Guid id, string reason = "")
    {
        var vendor = await _vendorRepository.GetByIdAsync(id);
        if (vendor == null)
            throw new NotFoundException("VendorNotFoundWithId", id);

        await _vendorRepository.SoftDeleteAsync(id, reason);
        return ControllerResponseBuilder.Success();
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

