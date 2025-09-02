using System.Net;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.PaymentMethods;
using Sky.Template.Backend.Contract.Responses.PaymentMethodResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Entities.System;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminPaymentMethodService
{
    [HasPermission(Permissions.PaymentMethods.View)]
    Task<BaseControllerResponse<PaymentMethodListPaginatedResponse>> GetFilteredPaginatedAsync(PaymentMethodFilterRequest request);
    [HasPermission(Permissions.PaymentMethods.View)]
    Task<BaseControllerResponse<PaymentMethodResponse>> GetByIdAsync(Guid id);
    [HasPermission(Permissions.PaymentMethods.Create)]
    Task<BaseControllerResponse<PaymentMethodResponse>> CreateAsync(CreatePaymentMethodRequest request);
    [HasPermission(Permissions.PaymentMethods.Update)]
    Task<BaseControllerResponse<PaymentMethodResponse>> UpdateAsync(Guid id, UpdatePaymentMethodRequest request);
    [HasPermission(Permissions.PaymentMethods.Delete)]
    Task<BaseControllerResponse> DeleteAsync(Guid id);
    [HasPermission(Permissions.PaymentMethods.Update)]
    Task<BaseControllerResponse> ToggleActivationAsync(Guid id);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
}

public class AdminPaymentMethodService : IAdminPaymentMethodService
{
    private readonly IPaymentMethodRepository _repository;
    private readonly IPaymentMethodTranslationRepository _translationRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdminPaymentMethodService(IPaymentMethodRepository repository, IPaymentMethodTranslationRepository translationRepository, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _translationRepository = translationRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    [HasPermission(Permissions.PaymentMethods.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.PaymentMethodsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<PaymentMethodListPaginatedResponse>> GetFilteredPaginatedAsync(PaymentMethodFilterRequest request)
    {
        var (entities, totalCount) = await _repository.GetFilteredPaginatedAsync(request);
        var lang = GetLanguageCode();
        var list = new List<PaymentMethodResponse>();
        foreach (var e in entities)
        {
            var tr = await _translationRepository.GetAsync(e.Id, lang);
            list.Add(Map(e, tr));
        }
        var paginated = new PaginatedData<PaymentMethodResponse>
        {
            Items = list,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
        return ControllerResponseBuilder.Success(new PaymentMethodListPaginatedResponse { PaymentMethods = paginated });
    }

    [HasPermission(Permissions.PaymentMethods.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.PaymentMethodsPrefix), ExpirationInMinutes = 60)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<PaymentMethodResponse>> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("PaymentMethodNotFound", id);
        var lang = GetLanguageCode();
        var tr = await _translationRepository.GetAsync(id, lang);
        return ControllerResponseBuilder.Success(Map(entity, tr));
    }

    [HasPermission(Permissions.PaymentMethods.Create)]
    [CacheRemove(nameof(CacheKeys.PaymentMethodsPattern))]
    [ValidationAspect(typeof(Validators.FluentValidation.PaymentMethods.CreatePaymentMethodRequestValidator))]
    public async Task<BaseControllerResponse<PaymentMethodResponse>> CreateAsync(CreatePaymentMethodRequest request)
    {
        if (!await IsCodeUniqueAsync(request.Code))
            throw new BusinessRulesException("PaymentMethod.CodeAlreadyExists");
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var entity = new PaymentMethodEntity
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            SupportedCurrency = request.SupportedCurrency,
            Type = request.Type,
            Status = request.Status,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };
        var created = await _repository.CreateAsync(entity);
        var lang = GetLanguageCode();
        await _translationRepository.UpsertAsync(new PaymentMethodTranslationEntity
        {
            PaymentMethodId = created.Id,
            LanguageCode = lang,
            Name = request.Name,
            Description = request.Description
        });
        return ControllerResponseBuilder.Success(Map(created, null), "Created", HttpStatusCode.Created);
    }

    [HasPermission(Permissions.PaymentMethods.Update)]
    [CacheRemove(nameof(CacheKeys.PaymentMethodsPattern))]
    [ValidationAspect(typeof(Validators.FluentValidation.PaymentMethods.UpdatePaymentMethodRequestValidator))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<PaymentMethodResponse>> UpdateAsync(Guid id, UpdatePaymentMethodRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("PaymentMethodNotFound", id);
        if (request.Code != null && request.Code != entity.Code && !await IsCodeUniqueAsync(request.Code, id))
            throw new BusinessRulesException("PaymentMethod.CodeAlreadyExists");
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        if (request.Name != null) entity.Name = request.Name;
        if (request.Code != null) entity.Code = request.Code;
        if (request.Description != null) entity.Description = request.Description;
        if (request.SupportedCurrency != null) entity.SupportedCurrency = request.SupportedCurrency;
        if (request.Type != null) entity.Type = request.Type;
        if (request.Status != null) entity.Status = request.Status;
        if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;
        var updated = await _repository.UpdateAsync(entity);
        var lang = GetLanguageCode();
        await _translationRepository.UpsertAsync(new PaymentMethodTranslationEntity
        {
            PaymentMethodId = updated.Id,
            LanguageCode = lang,
            Name = updated.Name,
            Description = updated.Description
        });
        return ControllerResponseBuilder.Success(Map(updated, null));
    }

    [HasPermission(Permissions.PaymentMethods.Delete)]
    [CacheRemove(nameof(CacheKeys.PaymentMethodsPattern))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("PaymentMethodNotFound", id);
        await _repository.SoftDeleteAsync(id);
        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.PaymentMethods.Update)]
    [CacheRemove(nameof(CacheKeys.PaymentMethodsPattern))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> ToggleActivationAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("PaymentMethodNotFound", id);
        entity.IsActive = !entity.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = _httpContextAccessor.HttpContext.GetUserId();
        await _repository.UpdateAsync(entity);
        return ControllerResponseBuilder.Success();
    }

    public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
        => await _repository.IsCodeUniqueAsync(code, excludeId);

    private string GetLanguageCode()
    {
        return _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString()?.Split(',').FirstOrDefault()?.ToLower() ?? "en";
    }

    private static PaymentMethodResponse Map(PaymentMethodEntity entity, PaymentMethodTranslationEntity? tr) => new()
    {
        Id = entity.Id,
        Name = tr?.Name ?? entity.Name,
        Code = entity.Code,
        Description = tr?.Description ?? entity.Description,
        IsActive = entity.IsActive,
        SupportedCurrency = entity.SupportedCurrency,
        Type = entity.Type,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy
    };
}
