using AutoMapper;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Application.Services.System;
using Sky.Template.Backend.Contract.Requests.BrandRequests;
using Sky.Template.Backend.Contract.Responses.BrandResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Entities.Brand;
using Sky.Template.Backend.Infrastructure.Repositories;
using System.Net;

namespace Sky.Template.Backend.Application.Services.Vendor;

public interface IVendorBrandService
{
    Task<BaseControllerResponse<BrandListPaginatedResponse>> GetPagedAsync(BrandFilterRequest request);
    Task<BaseControllerResponse<BrandDto>> GetByIdAsync(Guid id);
    Task<BaseControllerResponse<Guid>> CreateAsync(CreateBrandRequest request);
}

public class VendorBrandService : IVendorBrandService
{
    private readonly IBrandRepository _brandRepository;
    private readonly ISettingsService _settingsService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public VendorBrandService(IBrandRepository brandRepository, ISettingsService settingsService, IHttpContextAccessor httpContextAccessor, IMapper mapper)
    {
        _brandRepository = brandRepository;
        _settingsService = settingsService;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    [HasPermission(Permissions.Brands.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.BrandsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<BrandListPaginatedResponse>> GetPagedAsync(BrandFilterRequest request)
    {
        var lang = GetLanguageCode();
        var (items, total) = await _brandRepository.GetPagedAsync(request, lang, true);
        var dtos = _mapper.Map<List<BrandListItemDto>>(items);
        var paginated = new PaginatedData<BrandListItemDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPage = (int)Math.Ceiling((double)total / request.PageSize)
        };
        return ControllerResponseBuilder.Success(new BrandListPaginatedResponse { Brands = paginated });
    }

    [HasPermission(Permissions.Brands.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.BrandsPrefix), ExpirationInMinutes = 300)]
    public async Task<BaseControllerResponse<BrandDto>> GetByIdAsync(Guid id)
    {
        var brand = await _brandRepository.GetWithTranslationsAsync(id);
        if (brand == null || !string.Equals(brand.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
            return ControllerResponseBuilder.Failure<BrandDto>("Error.Brand.NotFound", HttpStatusCode.NotFound);
        var dto = _mapper.Map<BrandDto>(brand);
        return ControllerResponseBuilder.Success(dto);
    }

    [HasPermission(Permissions.Brands.VendorCreate)]
    [ValidationAspect(typeof(Validators.FluentValidation.Brand.CreateBrandRequestValidator))]
    [InvalidateCache(CacheKeys.BrandsPattern)]
    public async Task<BaseControllerResponse<Guid>> CreateAsync(CreateBrandRequest request)
    {
        var allow = await _settingsService.GetEffectiveSettingAsync("Settings:AllowVendorBrandCreate");
        if (!string.Equals(allow.Data, "true", StringComparison.OrdinalIgnoreCase))
            throw new BusinessRulesException("Error.Brand.VendorCreateDisabled");
        if (await _brandRepository.CodeExistsAsync(request.Code))
            throw new BusinessRulesException("Error.Brand.Slug.Exists");
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var brand = new BrandEntity
        {
            Id = Guid.NewGuid(),
            Slug = request.Code,
            LogoUrl = request.LogoUrl,
            Status = "INACTIVE",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        await _brandRepository.CreateAsync(brand);
        await _brandRepository.UpsertTranslationsAsync(brand.Id, request.Translations.Select(t => new BrandTranslationEntity
        {
            BrandId = brand.Id,
            LanguageCode = t.LanguageCode,
            Name = t.Name,
            Description = t.Description
        }));
        return ControllerResponseBuilder.Success(brand.Id, "Brand.Created", HttpStatusCode.Created);
    }

    private string GetLanguageCode()
        => _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString()?.Split(',').FirstOrDefault()?.ToLower() ?? "en";
}
