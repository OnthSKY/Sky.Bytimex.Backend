using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Core.Helpers;
using Sky.Template.Backend.Infrastructure.Entities.System;
using Sky.Template.Backend.Infrastructure.Entities.Vendor;
using Sky.Template.Backend.Infrastructure.Repositories.System;
using Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;
using System.Linq;
using System.Collections.Generic;

namespace Sky.Template.Backend.Application.Services.System;

public interface IProductSettingsService
{
    Task<GlobalProductSettings> GetGlobalSettingsAsync();
    Task<VendorProductSettings?> GetVendorSettingsAsync(Guid vendorId);
    Task<ProductSettings> GetEffectiveSettingsAsync(Guid vendorId);
    Task<BaseControllerResponse> UpdateGlobalSettingsAsync(GlobalProductSettings settings);
    Task<BaseControllerResponse> UpsertVendorSettingsAsync(Guid vendorId, VendorProductSettings settings);
}

public class ProductSettingsService : IProductSettingsService
{
    private readonly ISettingRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly string[] Keys =
    {
        "MAINTENANCE_MODE",
        "MAX_PRODUCT_COUNT_PER_VENDOR",
        "REQUIRE_VENDOR_KYC_FOR_PUBLISHING",
        "ALLOW_PRODUCT_DELETION"
    };

    public ProductSettingsService(ISettingRepository repository, IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.ProductGlobalSettingsPrefix), ExpirationInMinutes = 60)]
    public async Task<GlobalProductSettings> GetGlobalSettingsAsync()
    {
        var dict = await _repository.GetSettingsAsync(Keys);
        return new GlobalProductSettings
        {
            MaintenanceMode = dict.TryGetValue("MAINTENANCE_MODE", out var m) && bool.TryParse(m, out var mm) && mm,
            MaxProductCountPerVendor = dict.TryGetValue("MAX_PRODUCT_COUNT_PER_VENDOR", out var c) && int.TryParse(c, out var mc) ? mc : 0,
            RequireVendorKycForPublishing = dict.TryGetValue("REQUIRE_VENDOR_KYC_FOR_PUBLISHING", out var k) && bool.TryParse(k, out var rk) && rk,
            AllowProductDeletion = !dict.TryGetValue("ALLOW_PRODUCT_DELETION", out var d) || bool.TryParse(d, out var ad) && ad
        };
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.ProductVendorSettingsPrefix), ExpirationInMinutes = 60)]
    public async Task<VendorProductSettings?> GetVendorSettingsAsync(Guid vendorId)
    {
        var list = await _repository.GetVendorSettingsAsync(vendorId);
        var dict = list.ToDictionary(s => s.Key, s => s.Value);
        if (dict.Count == 0)
            return null;
        return new VendorProductSettings
        {
            MaintenanceMode = dict.TryGetValue("MAINTENANCE_MODE", out var m) && bool.TryParse(m, out var mm) ? mm : null,
            MaxProductCountPerVendor = dict.TryGetValue("MAX_PRODUCT_COUNT_PER_VENDOR", out var c) && int.TryParse(c, out var mc) ? mc : null,
            RequireVendorKycForPublishing = dict.TryGetValue("REQUIRE_VENDOR_KYC_FOR_PUBLISHING", out var k) && bool.TryParse(k, out var rk) ? rk : null,
            AllowProductDeletion = dict.TryGetValue("ALLOW_PRODUCT_DELETION", out var d) && bool.TryParse(d, out var ad) ? ad : null
        };
    }

    public async Task<ProductSettings> GetEffectiveSettingsAsync(Guid vendorId)
    {
        var global = await GetGlobalSettingsAsync();
        var vendor = await GetVendorSettingsAsync(vendorId);
        return ProductSettingsHelper.BuildEffective(global, vendor);
    }

    [HasPermission(Permissions.Settings.Manage)]
    [InvalidateCache(CacheKeys.ProductGlobalSettingsPrefix)]
    public async Task<BaseControllerResponse> UpdateGlobalSettingsAsync(GlobalProductSettings settings)
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var now = DateTime.UtcNow;

        var values = new Dictionary<string, string>
        {
            { "MAINTENANCE_MODE", settings.MaintenanceMode.ToString().ToLowerInvariant() },
            { "MAX_PRODUCT_COUNT_PER_VENDOR", settings.MaxProductCountPerVendor.ToString() },
            { "REQUIRE_VENDOR_KYC_FOR_PUBLISHING", settings.RequireVendorKycForPublishing.ToString().ToLowerInvariant() },
            { "ALLOW_PRODUCT_DELETION", settings.AllowProductDeletion.ToString().ToLowerInvariant() }
        };

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            foreach (var (key, value) in values)
            {
                var entity = new SystemSettingEntity
                {
                    Key = key,
                    Group = string.Empty,
                    Value = value,
                    IsPublic = false,
                    CreatedAt = now,
                    CreatedBy = userId,
                    UpdatedAt = now,
                    UpdatedBy = userId
                };
                await _repository.UpsertGlobalSettingAsync(entity, _unitOfWork.Connection, _unitOfWork.Transaction);
            }
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.Settings.Override)]
    [InvalidateCache(CacheKeys.ProductVendorSettingsPattern)]
    public async Task<BaseControllerResponse> UpsertVendorSettingsAsync(Guid vendorId, VendorProductSettings settings)
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var now = DateTime.UtcNow;

        var map = new Dictionary<string, string?>
        {
            { "MAINTENANCE_MODE", settings.MaintenanceMode?.ToString().ToLowerInvariant() },
            { "MAX_PRODUCT_COUNT_PER_VENDOR", settings.MaxProductCountPerVendor?.ToString() },
            { "REQUIRE_VENDOR_KYC_FOR_PUBLISHING", settings.RequireVendorKycForPublishing?.ToString().ToLowerInvariant() },
            { "ALLOW_PRODUCT_DELETION", settings.AllowProductDeletion?.ToString().ToLowerInvariant() }
        };

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            foreach (var (key, value) in map.Where(kv => kv.Value != null))
            {
                var entity = new VendorSettingEntity
                {
                    VendorId = vendorId,
                    Key = key,
                    Value = value!,
                    CreatedAt = now,
                    CreatedBy = userId,
                    UpdatedAt = now,
                    UpdatedBy = userId
                };
                await _repository.UpsertVendorSettingAsync(entity, _unitOfWork.Connection, _unitOfWork.Transaction);
            }
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        return ControllerResponseBuilder.Success();
    }
}
