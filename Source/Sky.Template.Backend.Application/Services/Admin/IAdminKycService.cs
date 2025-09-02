using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Contract.Requests.Kyc;
using Sky.Template.Backend.Contract.Responses.Kyc;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Entities.Kyc;
using Sky.Template.Backend.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminKycService
{
    Task<BaseControllerResponse<KycStatusResponse>> ApproveVerificationAsync(KycApprovalRequest request);
    Task<BaseControllerResponse<KycStatusResponse>> GetStatusAsync(Guid userId);
}

public class AdminKycService : IAdminKycService
{
    private readonly IKycVerificationRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICacheService _cacheService;

    public AdminKycService(IKycVerificationRepository repository, IHttpContextAccessor accessor, ICacheService cacheService)
    {
        _repository = repository;
        _httpContextAccessor = accessor;
        _cacheService = cacheService;
    }

    [HasPermission(Permissions.Kyc.Verify)]
    public async Task<BaseControllerResponse<KycStatusResponse>> ApproveVerificationAsync(KycApprovalRequest request)
    {
        var entity = await _repository.GetByIdAsync(request.VerificationId);
        if (entity == null) throw new NotFoundException("KycNotFound", request.VerificationId);
        var reviewerId = _httpContextAccessor.HttpContext.GetUserId();
        entity.Status = request.Approve ? "VERIFIED" : "REJECTED";
        entity.Reason = request.Reason;
        entity.ReviewedBy = reviewerId;
        entity.ReviewedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = reviewerId;
        await _repository.UpdateAsync(entity);
        await _cacheService.RemoveAsync($"{CacheKeys.UserKycPrefix}:{entity.UserId}");
        return ControllerResponseBuilder.Success(new KycStatusResponse
        {
            Id = entity.Id,
            UserId = entity.UserId,
            NationalId = entity.NationalId,
            Country = entity.Country,
            DocumentType = entity.DocumentType,
            DocumentNumber = entity.DocumentNumber,
            DocumentExpiryDate = entity.DocumentExpiryDate,
            SelfieUrl = entity.SelfieUrl,
            DocumentFrontUrl = entity.DocumentFrontUrl,
            DocumentBackUrl = entity.DocumentBackUrl,
            Status = entity.Status,
            Reason = entity.Reason,
            ReviewedBy = entity.ReviewedBy,
            ReviewedAt = entity.ReviewedAt,
            IsDeleted = entity.IsDeleted,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
            DeletedAt = entity.DeletedAt,
            DeletedBy = entity.DeletedBy,
            DeleteReason = entity.DeleteReason
        });
    }

    public async Task<BaseControllerResponse<KycStatusResponse>> GetStatusAsync(Guid userId)
    {
        var entity = await _repository.GetByUserIdAsync(userId);
        if (entity == null) throw new NotFoundException("KycNotFoundForUser", userId);
        return ControllerResponseBuilder.Success(new KycStatusResponse
        {
            Id = entity.Id,
            UserId = entity.UserId,
            NationalId = entity.NationalId,
            Country = entity.Country,
            DocumentType = entity.DocumentType,
            DocumentNumber = entity.DocumentNumber,
            DocumentExpiryDate = entity.DocumentExpiryDate,
            SelfieUrl = entity.SelfieUrl,
            DocumentFrontUrl = entity.DocumentFrontUrl,
            DocumentBackUrl = entity.DocumentBackUrl,
            Status = entity.Status,
            Reason = entity.Reason,
            ReviewedBy = entity.ReviewedBy,
            ReviewedAt = entity.ReviewedAt,
            IsDeleted = entity.IsDeleted,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
            DeletedAt = entity.DeletedAt,
            DeletedBy = entity.DeletedBy,
            DeleteReason = entity.DeleteReason
        });
    }
}

