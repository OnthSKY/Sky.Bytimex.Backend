using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Responses.SettingResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Entities.System;
using Sky.Template.Backend.Infrastructure.Entities.Vendor;
using Sky.Template.Backend.Infrastructure.Repositories.System;
using System.Collections.Generic;
using System.Linq;

namespace Sky.Template.Backend.Application.Services.System;

public interface ISettingsService
{
    Task<BaseControllerResponse<string?>> GetEffectiveSettingAsync(string key, Guid? vendorId = null);
    Task<BaseControllerResponse<List<SettingDto>>> GetAllGlobalSettingsAsync();
    Task<BaseControllerResponse<List<SettingDto>>> GetVendorOverridesAsync(Guid vendorId);
    Task<BaseControllerResponse> UpdateGlobalSettingAsync(string key, string value);
    Task<BaseControllerResponse> UpsertVendorSettingAsync(Guid vendorId, string key, string value);
    Task<IDictionary<string, string>> GetSettingsAsync(IEnumerable<string> keys);
}

public class SettingsService : ISettingsService
{
    private readonly ISettingRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SettingsService(ISettingRepository repository, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.SettingsPrefix), ExpirationInMinutes = 120)]
    public async Task<BaseControllerResponse<string?>> GetEffectiveSettingAsync(string key, Guid? vendorId = null)
    {
        string? value = null;

        if (vendorId.HasValue)
        {
            var vendorSetting = await _repository.GetVendorSettingAsync(vendorId.Value, key);
            value = vendorSetting?.Value;
        }

        if (value is null)
        {
            var global = await _repository.GetGlobalSettingAsync(key);
            value = global?.Value;
        }

        return ControllerResponseBuilder.Success(data:value);
    }

    [HasPermission(Permissions.Settings.Manage)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.SettingsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<List<SettingDto>>> GetAllGlobalSettingsAsync()
    {
        var entities = await _repository.GetAllGlobalSettingsAsync();
        var list = entities.Select(e => new SettingDto
        {
            Key = e.Key,
            Group = e.Group,
            Value = e.Value,
            Description = e.Description,
            IsPublic = e.IsPublic
        }).ToList();
        return ControllerResponseBuilder.Success(list);
    }

    [HasPermission(Permissions.Settings.Override)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.VendorOverridesPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<List<SettingDto>>> GetVendorOverridesAsync(Guid vendorId)
    {
        var items = await _repository.GetVendorSettingsAsync(vendorId);
        return ControllerResponseBuilder.Success(items.ToList());
    }

    [HasPermission(Permissions.Settings.Manage)]
    [InvalidateCache(CacheKeys.SettingsPattern)]
    public async Task<BaseControllerResponse> UpdateGlobalSettingAsync(string key, string value)
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var now = DateTime.UtcNow;
        var entity = new SystemSettingEntity
        {
            Key = key,
            Group = string.Empty,
            Value = value,
            Description = null,
            IsPublic = false,
            CreatedAt = now,
            CreatedBy = userId,
            UpdatedAt = now,
            UpdatedBy = userId
        };
        await _repository.UpsertGlobalSettingAsync(entity);
        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.Settings.Override)]
    [InvalidateCache(CacheKeys.SettingsPattern)]
    [InvalidateCache(CacheKeys.VendorOverridesPattern)]
    public async Task<BaseControllerResponse> UpsertVendorSettingAsync(Guid vendorId, string key, string value)
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var now = DateTime.UtcNow;
        var entity = new VendorSettingEntity
        {
            VendorId = vendorId,
            Key = key,
            Value = value,
            CreatedAt = now,
            CreatedBy = userId,
            UpdatedAt = now,
            UpdatedBy = userId
        };

        await _repository.UpsertVendorSettingAsync(entity);
        return ControllerResponseBuilder.Success();
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.SettingsPrefix), ExpirationInMinutes = 1)]
    public Task<IDictionary<string, string>> GetSettingsAsync(IEnumerable<string> keys)
        => _repository.GetSettingsAsync(keys);
}
