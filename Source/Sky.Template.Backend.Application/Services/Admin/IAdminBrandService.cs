using AutoMapper;
using Microsoft.AspNetCore.Http;
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
using System.Linq;
using System;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminBrandService
{
    Task<BaseControllerResponse<BrandListPaginatedResponse>> GetPagedAsync(BrandFilterRequest request, bool includeInactive = true);
    Task<BaseControllerResponse<BrandDto>> GetByIdAsync(Guid id);
    Task<BaseControllerResponse<Guid>> CreateAsync(CreateBrandRequest request);
    Task<BaseControllerResponse> UpdateAsync(UpdateBrandRequest request);
    Task<BaseControllerResponse> SoftDeleteAsync(Guid id);
    Task<BaseControllerResponse> HardDeleteAsync(Guid id);
}

public class AdminBrandService : IAdminBrandService
{
    private readonly IBrandRepository _brandRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public AdminBrandService(IBrandRepository brandRepository, IHttpContextAccessor httpContextAccessor, IMapper mapper)
    {
        _brandRepository = brandRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    [HasPermission(Permissions.Brands.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.BrandsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<BrandListPaginatedResponse>> GetPagedAsync(BrandFilterRequest request, bool includeInactive = true)
    {
        var lang = GetLanguageCode();
        var (items, total) = await _brandRepository.GetPagedAsync(request, lang, !includeInactive);
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
        if (brand == null)
            return ControllerResponseBuilder.Failure<BrandDto>("Error.Brand.NotFound", HttpStatusCode.NotFound);
        var dto = _mapper.Map<BrandDto>(brand);
        return ControllerResponseBuilder.Success(dto);
    }

    [HasPermission(Permissions.Brands.Create)]
    [ValidationAspect(typeof(Validators.FluentValidation.Brand.CreateBrandRequestValidator))]
    [InvalidateCache(CacheKeys.BrandsPattern)]
    public async Task<BaseControllerResponse<Guid>> CreateAsync(CreateBrandRequest request)
    {
        if (await _brandRepository.CodeExistsAsync(request.Code))
            throw new BusinessRulesException("Error.Brand.Slug.Exists");
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var brand = new BrandEntity
        {
            Id = Guid.NewGuid(),
            Slug = request.Code,
            LogoUrl = request.LogoUrl,
            Status = "ACTIVE",
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

    [HasPermission(Permissions.Brands.Update)]
    [ValidationAspect(typeof(Validators.FluentValidation.Brand.UpdateBrandRequestValidator))]
    [InvalidateCache(CacheKeys.BrandsPattern)]
    public async Task<BaseControllerResponse> UpdateAsync(UpdateBrandRequest request)
    {
        var brand = await _brandRepository.GetWithTranslationsAsync(request.Id);
        if (brand == null)
            return ControllerResponseBuilder.Failure("Error.Brand.NotFound", HttpStatusCode.NotFound);
        if (await _brandRepository.CodeExistsAsync(request.Code, request.Id))
            throw new BusinessRulesException("Error.Brand.Slug.Exists");
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        brand.Slug = request.Code;
        brand.LogoUrl = request.LogoUrl;
        brand.Status = request.Status;
        brand.UpdatedAt = DateTime.UtcNow;
        brand.UpdatedBy = userId;
        await _brandRepository.UpdateAsync(brand);
        await _brandRepository.UpsertTranslationsAsync(brand.Id, request.Translations.Select(t => new BrandTranslationEntity
        {
            BrandId = brand.Id,
            LanguageCode = t.LanguageCode,
            Name = t.Name,
            Description = t.Description
        }));
        return ControllerResponseBuilder.Success(messageKey: "Brand.Updated");
    }

    [HasPermission(Permissions.Brands.Delete)]
    [InvalidateCache(CacheKeys.BrandsPattern)]
    public async Task<BaseControllerResponse> SoftDeleteAsync(Guid id)
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        await _brandRepository.SoftDeleteAsync(id, userId);
        return ControllerResponseBuilder.Success(messageKey:"Brand.Deleted");
    }

    [HasPermission(Permissions.Brands.HardDelete)]
    [InvalidateCache(CacheKeys.BrandsPattern)]
    public async Task<BaseControllerResponse> HardDeleteAsync(Guid id)
    {
        await _brandRepository.HardDeleteAsync(id);
        return ControllerResponseBuilder.Success(messageKey: "Brand.HardDeleted");
    }

    private string GetLanguageCode()
        => _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString()?.Split(',').FirstOrDefault()?.ToLower() ?? "en";
}
