using Sky.Template.Backend.Contract.Responses.Storefront;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Infrastructure.Entities.Vendor;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.Storefront;

public interface IStorefrontVendorService
{
    Task<BaseControllerResponse<VendorListPaginatedResponse>> GetVendorsAsync(GridRequest request);
    Task<BaseControllerResponse<VendorDetailResponse>> GetVendorDetailAsync(string? slug, Guid? id);
}

public class StorefrontVendorService : IStorefrontVendorService
{
    private readonly IStorefrontVendorRepository _repository;

    public StorefrontVendorService(IStorefrontVendorRepository repository)
    {
        _repository = repository;
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.StorefrontVendorsPrefix), ExpirationInMinutes = 2)]
    public async Task<BaseControllerResponse<VendorListPaginatedResponse>> GetVendorsAsync(GridRequest request)
    {
        var (vendors, total) = await _repository.GetVendorsAsync(request);
        var items = vendors.Select(MapListItem).ToList();
        var paginated = new PaginatedData<VendorListItemResponse>
        {
            Items = items,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPage = (int)Math.Ceiling((double)total / request.PageSize)
        };
        return ControllerResponseBuilder.Success(new VendorListPaginatedResponse { Vendors = paginated });
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.StorefrontVendorDetailPrefix), ExpirationInMinutes = 2)]
    public async Task<BaseControllerResponse<VendorDetailResponse>> GetVendorDetailAsync(string? slug, Guid? id)
    {
        VendorEntity? vendor = null;
        if (!string.IsNullOrWhiteSpace(slug))
            vendor = await _repository.GetActiveBySlugAsync(slug);
        else if (id.HasValue)
            vendor = await _repository.GetActiveByIdAsync(id.Value);

        if (vendor == null)
            throw new NotFoundException("VendorNotFound");

        return ControllerResponseBuilder.Success(MapDetail(vendor));
    }

    private static VendorListItemResponse MapListItem(VendorWithProductCountEntity v) => new()
    {
        Id = v.Id,
        Name = v.Name,
        Slug = v.Slug,
        ShortDescription = v.ShortDescription,
        LogoUrl = v.LogoUrl,
        RatingAvg = v.RatingAvg,
        RatingCount = v.RatingCount,
        ProductCount = v.ProductCount
    };

    private static VendorDetailResponse MapDetail(VendorEntity v) => new()
    {
        Id = v.Id,
        Name = v.Name,
        Slug = v.Slug,
        ShortDescription = v.ShortDescription,
        LogoUrl = v.LogoUrl,
        BannerUrl = v.BannerUrl,
        RatingAvg = v.RatingAvg,
        RatingCount = v.RatingCount,
        CreatedAt = v.CreatedAt
    };
}
