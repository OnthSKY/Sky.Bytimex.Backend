using System.Collections.Generic;
using System.Net;
using Sky.Template.Backend.Application.Events;
using Sky.Template.Backend.Application.Events.Vendor;
using Sky.Template.Backend.Contract.Requests.Kyc;
using Sky.Template.Backend.Contract.Responses.Kyc;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Entities.Kyc;
using Sky.Template.Backend.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;

namespace Sky.Template.Backend.Application.Services.Vendor;

public interface IVendorKycService
{
    [HasPermission(Permissions.Kyc.Manage)]
    Task<BaseControllerResponse<VendorKycStatusResponse>> SubmitVendorKycAsync(VendorKycRequest request);
    [HasPermission(Permissions.Kyc.Manage)]
    Task<BaseControllerResponse<VendorKycStatusResponse>> GetVendorKycAsync(Guid vendorId);
    [HasPermission(Permissions.Kyc.Manage)]
    Task<BaseControllerResponse<VendorKycStatusResponse>> ReviewVendorKycAsync(VendorKycReviewRequest request);
}

public class VendorKycService : IVendorKycService
{
    private readonly IVendorKycRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEventPublisher? _eventPublisher;

    public VendorKycService(IVendorKycRepository repository, IHttpContextAccessor httpContextAccessor, IEventPublisher? eventPublisher = null)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
        _eventPublisher = eventPublisher;
    }

    private Guid GetUserId() => _httpContextAccessor.HttpContext.GetUserId();

    [HasPermission(Permissions.Kyc.Manage)]
    [ValidationAspect(typeof(Validators.FluentValidation.Kyc.VendorKycRequestValidator))]
    [InvalidateCache(CacheKeys.VendorKycPattern)]
    public async Task<BaseControllerResponse<VendorKycStatusResponse>> SubmitVendorKycAsync(VendorKycRequest request)
    {
        var vendorId = GetUserId();
        var entity = new VendorKycEntity
        {
            Id = Guid.NewGuid(),
            VendorId = vendorId,
            LegalName = request.LegalName,
            TaxId = request.TaxId,
            Documents = string.Join(',', request.Documents ?? new List<string>()),
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = vendorId,
            IsDeleted = false
        };
        await _repository.CreateAsync(entity);
        if (_eventPublisher != null)
            await _eventPublisher.PublishAsync(new VendorKycSubmittedEvent(vendorId));
        return ControllerResponseBuilder.Success(Map(entity), "Created", HttpStatusCode.Created);
    }

    [HasPermission(Permissions.Kyc.Manage)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.VendorKycPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<VendorKycStatusResponse>> GetVendorKycAsync(Guid vendorId)
    {
        var entity = await _repository.GetByVendorIdAsync(vendorId) ?? throw new NotFoundException("VendorKycNotFound", vendorId);
        return ControllerResponseBuilder.Success(Map(entity));
    }

    [HasPermission(Permissions.Kyc.Manage)]
    [ValidationAspect(typeof(Validators.FluentValidation.Kyc.VendorKycReviewRequestValidator))]
    [InvalidateCache(CacheKeys.VendorKycPattern)]
    public async Task<BaseControllerResponse<VendorKycStatusResponse>> ReviewVendorKycAsync(VendorKycReviewRequest request)
    {
        var reviewerId = GetUserId();
        var entity = await _repository.GetByVendorIdAsync(request.VendorId) ?? throw new NotFoundException("VendorKycNotFound", request.VendorId);
        entity.Status = request.Approve ? "APPROVED" : "REJECTED";
        entity.RejectionReason = request.Approve ? null : request.RejectionReason;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = reviewerId;
        await _repository.UpdateAsync(entity);
        return ControllerResponseBuilder.Success(Map(entity));
    }

    private static VendorKycStatusResponse Map(VendorKycEntity entity) => new()
    {
        Id = entity.Id,
        Status = entity.Status,
        LastUpdatedAt = entity.UpdatedAt ?? entity.CreatedAt,
        RejectionReason = entity.RejectionReason
    };
}
