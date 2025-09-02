using Sky.Template.Backend.Core.Constants;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.Products;
using Sky.Template.Backend.Contract.Responses.ProductResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities.Product;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.User;

public interface IUserProductService
{
    Task<BaseControllerResponse<ProductListResponse>> GetAllProductsAsync();
    Task<BaseControllerResponse<SingleProductResponse>> GetProductByIdAsync(Guid id);
    Task<BaseControllerResponse<SingleProductResponse>> GetProductBySkuAsync(string sku);
    Task<BaseControllerResponse<ProductListPaginatedResponse>> GetFilteredPaginatedProductsAsync(ProductFilterRequest request);
    Task<BaseControllerResponse<ProductStockDto>> GetProductStockStatusAsync(Guid productId);
    Task<BaseControllerResponse<ProductVariantListPaginatedResponse>> GetProductVariantsAsync(Guid productId, ProductVariantFilterRequest request);
}

public class UserProductService : IUserProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IProductTranslationRepository _translationRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserProductService(IProductRepository productRepository,
        IProductTranslationRepository translationRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _productRepository = productRepository;
        _translationRepository = translationRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.ProductsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<ProductListResponse>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        var lang = GetLanguageCode();
        var list = new List<SingleProductResponse>();
        foreach (var p in products)
        {
            var tr = await _translationRepository.GetAsync(p.Id, lang);
            list.Add(MapToResponse(p, tr?.Name ?? string.Empty, tr?.Description));
        }
        var response = new ProductListResponse
        {
            Products = list,
            TotalCount = list.Count
        };
        return ControllerResponseBuilder.Success(response);
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.ProductsPrefix), ExpirationInMinutes = 60)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<SingleProductResponse>> GetProductByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            throw new NotFoundException("ProductNotFound", id);
        var lang = GetLanguageCode();
        var tr = await _translationRepository.GetAsync(product.Id, lang);
        var response = MapToResponse(product, tr?.Name ?? string.Empty, tr?.Description);
        return ControllerResponseBuilder.Success(response);
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.ProductsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<SingleProductResponse>> GetProductBySkuAsync(string sku)
    {
        var product = await _productRepository.GetProductBySkuAsync(sku);
        if (product == null)
            throw new NotFoundException("ProductNotFoundWithSku", sku);
        var lang = GetLanguageCode();
        var tr = await _translationRepository.GetAsync(product.Id, lang);
        var response = MapToResponse(product, tr?.Name ?? string.Empty, tr?.Description);
        return ControllerResponseBuilder.Success(response);
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.ProductsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<ProductListPaginatedResponse>> GetFilteredPaginatedProductsAsync(ProductFilterRequest request)
    {
        var (data, totalCount) = await _productRepository.GetLocalizedProductsAsync(request, true);
        var mapped = data.Select(MapToResponse).ToList();

        var paginated = new PaginatedData<SingleProductResponse>
        {
            Items = mapped,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };

        return ControllerResponseBuilder.Success(new ProductListPaginatedResponse
        {
            Products = paginated
        });
    }


    [HasPermission(Permissions.Products.Read)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.StockStatusPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<ProductStockDto>> GetProductStockStatusAsync(Guid productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new NotFoundException("ProductNotFound", productId);

        var dto = new ProductStockDto
        {
            ProductId = product.Id,
            StockQuantity = product.StockQuantity ?? 0
        };

        return ControllerResponseBuilder.Success(dto);
    }

    public async Task<BaseControllerResponse<ProductVariantListPaginatedResponse>> GetProductVariantsAsync(Guid productId, ProductVariantFilterRequest request)
    {
        var (data, totalCount) = await _productRepository.GetProductVariantsAsync(productId, request);
        var mapped = data.Select(v => new ProductVariantDto
        {
            Id = v.Id,
            ProductId = v.ProductId,
            Sku = v.Sku,
            Price = v.Price,
            StockQuantity = v.StockQuantity,
            IsActive = v.IsActive,
            CreatedAt = v.CreatedAt
        }).ToList();

        var paginated = new PaginatedData<ProductVariantDto>
        {
            Items = mapped,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };

        return ControllerResponseBuilder.Success(new ProductVariantListPaginatedResponse
        {
            Variants = paginated
        });
    }

    private static SingleProductResponse MapToResponse(ProductEntity product, string name, string? description) => new()
    {
        Id = product.Id,
        Slug = product.Slug ?? string.Empty,
        VendorId = product.VendorId,
        Name = name,
        Description = description,
        CategoryId = product.CategoryId,
        ProductType = product.ProductType,
        Price = product.Price,
        PrimaryImageUrl = null,
        Unit = product.Unit,
        Barcode = product.Barcode,
        StockQuantity = product.StockQuantity,
        IsStockTracked = product.IsStockTracked,
        Sku = product.Sku,
        IsDecimalQuantityAllowed = product.IsDecimalQuantityAllowed,
        Status = product.Status,
        IsDeleted = product.IsDeleted,
        CreatedAt = product.CreatedAt,
        CreatedBy = product.CreatedBy,
        UpdatedAt = product.UpdatedAt,
        UpdatedBy = product.UpdatedBy,
        DeletedAt = product.DeletedAt,
        DeletedBy = product.DeletedBy,
        DeleteReason = product.DeleteReason
    };

    private static SingleProductResponse MapToResponse(ProductLocalizedJoinEntity product) => new()
    {
        Id = product.Id,
        Slug = product.Slug,
        VendorId = product.VendorId,
        Name = product.Name,
        Description = product.Description,
        CategoryId = product.CategoryId,
        ProductType = product.ProductType,
        Price = product.Price,
        PrimaryImageUrl = product.PrimaryImageUrl,
        Unit = product.Unit,
        Barcode = product.Barcode,
        StockQuantity = product.StockQuantity,
        IsStockTracked = product.IsStockTracked,
        Sku = product.Sku,
        IsDecimalQuantityAllowed = product.IsDecimalQuantityAllowed,
        Status = product.Status,
        IsDeleted = product.IsDeleted,
        CreatedAt = product.CreatedAt,
        DeletedAt = product.DeletedAt,
        DeletedBy = product.DeletedBy,
        DeleteReason = product.DeleteReason
    };

    private string GetLanguageCode()
        => _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString()?.Split(',').FirstOrDefault()?.ToLower() ?? "en";
}
