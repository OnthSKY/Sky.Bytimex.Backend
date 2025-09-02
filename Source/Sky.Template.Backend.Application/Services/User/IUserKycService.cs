using Sky.Template.Backend.Contract.Requests.Kyc;
using Sky.Template.Backend.Contract.Responses.Kyc;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Entities.Kyc;
using Sky.Template.Backend.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;

namespace Sky.Template.Backend.Application.Services.User;

public interface IUserKycService
{
    Task<BaseControllerResponse<KycStatusResponse>> StartVerificationAsync(KycVerificationRequest request);
    Task<BaseControllerResponse<KycStatusResponse>> GetStatusAsync(Guid userId);
    [HasPermission(Permissions.Kyc.Resubmit)]
    Task ResubmitKycAsync(Guid userId, KycSubmissionRequest request);
    [HasPermission(Permissions.Kyc.Delete)]
    Task DeleteKycAsync(Guid userId, Guid kycId);
}

public class UserKycService : IUserKycService
{
    private readonly IKycVerificationRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICacheService _cacheService;
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();
    private static readonly HashSet<string> ValidDocumentTypes = new(new[] { "ID", "PASSPORT" });
    private const int CacheDuration = 60;

    public UserKycService(IKycVerificationRepository repository, IHttpContextAccessor accessor, ICacheService cacheService)
    {
        _repository = repository;
        _httpContextAccessor = accessor;
        _cacheService = cacheService;
    }

    public async Task<BaseControllerResponse<KycStatusResponse>> StartVerificationAsync(KycVerificationRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        if (!ValidDocumentTypes.Contains(request.DocumentType))
            throw new BusinessRulesException("InvalidDocumentType", request.DocumentType);

        var semaphore = _locks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        try
        {
            var existing = await _repository.GetByUserIdAsync(userId);
            if (existing != null)
            {
                existing.NationalId = request.NationalId;
                existing.Country = request.Country;
                existing.DocumentType = request.DocumentType;
                existing.DocumentNumber = request.DocumentNumber;
                existing.DocumentExpiryDate = request.DocumentExpiryDate;
                existing.SelfieUrl = request.SelfieUrl;
                existing.DocumentFrontUrl = request.DocumentFrontUrl;
                existing.DocumentBackUrl = request.DocumentBackUrl;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = userId;

                await _repository.UpdateAsync(existing);
                await _cacheService.RemoveAsync($"{CacheKeys.UserKycPrefix}:{userId}");
                return ControllerResponseBuilder.Success(Map(existing));
            }

            var entity = new KycVerificationEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                NationalId = request.NationalId,
                Country = request.Country,
                DocumentType = request.DocumentType,
                DocumentNumber = request.DocumentNumber,
                DocumentExpiryDate = request.DocumentExpiryDate,
                SelfieUrl = request.SelfieUrl,
                DocumentFrontUrl = request.DocumentFrontUrl,
                DocumentBackUrl = request.DocumentBackUrl,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                IsDeleted = false
            };
            await _repository.CreateAsync(entity);
            await _cacheService.RemoveAsync($"{CacheKeys.UserKycPrefix}:{userId}");
            return ControllerResponseBuilder.Success(Map(entity));
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<BaseControllerResponse<KycStatusResponse>> GetStatusAsync(Guid userId)
    {
        return await _cacheService.GetOrSetAsync(
            $"{CacheKeys.UserKycPrefix}:{userId}",
            async () =>
            {
                var entity = await _repository.GetByUserIdAsync(userId) ?? throw new NotFoundException("KycNotFoundForUser", userId);
                return ControllerResponseBuilder.Success(Map(entity));
            },
            new CacheEntryOptions
            {
                AbsoluteExpiration = TimeSpan.FromMinutes(CacheDuration)
            });
    }

    [HasPermission(Permissions.Kyc.Resubmit)]
    public async Task ResubmitKycAsync(Guid userId, KycSubmissionRequest request)
    {
        var entity = await _repository.GetByIdAsync(request.VerificationId);
        if (entity == null || entity.UserId != userId)
            throw new NotFoundException("KycNotFound", request.VerificationId);

        if (entity.Status != "REJECTED" && entity.Status != "EXPIRED")
            throw new BusinessRulesException("KycResubmissionNotAllowed", entity.Status);

        entity.NationalId = request.NationalId;
        entity.Country = request.Country;
        entity.DocumentType = request.DocumentType;
        entity.DocumentNumber = request.DocumentNumber;
        entity.DocumentExpiryDate = request.DocumentExpiryDate;
        if (request.Selfie != null) entity.SelfieUrl = request.Selfie.FileName;
        if (request.DocumentFront != null) entity.DocumentFrontUrl = request.DocumentFront.FileName;
        if (request.DocumentBack != null) entity.DocumentBackUrl = request.DocumentBack.FileName;
        entity.Status = "PENDING";
        entity.Reason = request.Reason;
        entity.ReviewedBy = null;
        entity.ReviewedAt = null;
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity);
        await _cacheService.RemoveAsync($"{CacheKeys.UserKycPrefix}:{userId}");
    }

    [HasPermission(Permissions.Kyc.Delete)]
    public async Task DeleteKycAsync(Guid userId, Guid kycId)
    {
        var entity = await _repository.GetByIdAsync(kycId);
        if (entity == null || entity.UserId != userId)
            throw new NotFoundException("KycNotFound", kycId);

        if (entity.Status != "PENDING" && entity.Status != "REJECTED")
            throw new BusinessRulesException("KycDeletionNotAllowed", entity.Status);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = userId;

        await _repository.UpdateAsync(entity);
        await _cacheService.RemoveAsync($"{CacheKeys.UserKycPrefix}:{userId}");
    }

    private static KycStatusResponse Map(KycVerificationEntity entity) => new()
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
    };
}

