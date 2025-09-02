using AutoMapper;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.BrandRequests;
using Sky.Template.Backend.Contract.Responses.BrandResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Infrastructure.Repositories;
using System.Net;

namespace Sky.Template.Backend.Application.Services.User;

public interface IBrandService
{
    Task<BaseControllerResponse<BrandListPaginatedResponse>> GetPagedAsync(BrandFilterRequest request);
    Task<BaseControllerResponse<BrandDto>> GetByIdAsync(Guid id);
}

public class BrandService : IBrandService
{
    private readonly IBrandRepository _brandRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BrandService(IBrandRepository brandRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _brandRepository = brandRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

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

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.BrandsPrefix), ExpirationInMinutes = 300)]
    public async Task<BaseControllerResponse<BrandDto>> GetByIdAsync(Guid id)
    {
        var brand = await _brandRepository.GetWithTranslationsAsync(id);
        if (brand == null || !string.Equals(brand.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
            return ControllerResponseBuilder.Failure<BrandDto>("Error.Brand.NotFound", HttpStatusCode.NotFound);
        var dto = _mapper.Map<BrandDto>(brand);
        return ControllerResponseBuilder.Success(dto);
    }

    private string GetLanguageCode()
        => _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString()?.Split(',').FirstOrDefault()?.ToLower() ?? "en";
}
