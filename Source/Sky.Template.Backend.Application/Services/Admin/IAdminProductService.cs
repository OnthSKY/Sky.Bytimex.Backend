using Sky.Template.Backend.Core.Constants;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.Products;
using Sky.Template.Backend.Contract.Responses.ProductResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Entities.Product;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminProductService
{
    Task<BaseControllerResponse<ProductListPaginatedDtoResponse>> GetAllProductsAsync(ProductFilterRequest request);
    Task<BaseControllerResponse<ProductDto>> GetProductByIdAsync(Guid productId);
    Task<BaseControllerResponse<ProductDto>> CreateProductAsync(CreateProductRequest request);
    Task<BaseControllerResponse<ProductDto>> UpdateProductAsync(Guid productId, UpdateProductRequest request);
    Task<BaseControllerResponse> HardDeleteProductAsync(Guid id);
    Task<BaseControllerResponse<Guid>> AddProductImageAsync(AddImageRequest request);
    Task<BaseControllerResponse> UpsertProductAttributeAsync(UpsertAttributeRequest request);
    Task<BaseControllerResponse<Guid>> CreateVariantAsync(CreateVariantRequest request);
    Task<BaseControllerResponse> SetVariantAttributeAsync(SetVariantAttributeRequest request);
}

public class AdminProductService : IAdminProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IProductTranslationRepository _translationRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdminProductService(IProductRepository productRepository,
        IProductTranslationRepository translationRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _productRepository = productRepository;
        _translationRepository = translationRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    [HasPermission(Permissions.Products.Read)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.ProductsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<ProductListPaginatedDtoResponse>> GetAllProductsAsync(ProductFilterRequest request)
    {
        var (data, totalCount) = await _productRepository.GetLocalizedProductsAsync(request);
        var mapped = data.Select(MapToDto).ToList();

        var paginated = new PaginatedData<ProductDto>
        {
            Items = mapped,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };

        return ControllerResponseBuilder.Success(new ProductListPaginatedDtoResponse
        {
            Products = paginated
        });
    }

    [HasPermission(Permissions.Products.Read)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.ProductsPrefix), ExpirationInMinutes = 60)]
    [EnsureUserIsValid(new[] { "productId" })]
    public async Task<BaseControllerResponse<ProductDto>> GetProductByIdAsync(Guid productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new NotFoundException("ProductNotFound", productId);

        var lang = GetLanguageCode();
        var tr = await _translationRepository.GetAsync(product.Id, lang);
        var dto = MapToDto(product, tr?.Name ?? string.Empty, tr?.Description);
        return ControllerResponseBuilder.Success(dto);
    }

    [HasPermission(Permissions.Products.Create)]
    [InvalidateCache(CacheKeys.ProductsPattern)]
    public async Task<BaseControllerResponse<ProductDto>> CreateProductAsync(CreateProductRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var product = new ProductEntity
        {
            VendorId = null,
            CategoryId = request.CategoryId,
            ProductType = request.ProductType,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            IsStockTracked = request.IsStockTracked,
            Sku = request.Sku,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };

        await _productRepository.CreateAsync(product);

        var lang = GetLanguageCode();
        await _translationRepository.UpsertAsync(new ProductTranslationEntity
        {
            ProductId = product.Id,
            LanguageCode = lang,
            Name = request.Name,
            Description = request.Description
        });

        var dto = MapToDto(product, request.Name, request.Description);
        return ControllerResponseBuilder.Success(dto, "ProductCreatedSuccessfully");
    }

    [HasPermission(Permissions.Products.Update)]
    [InvalidateCache(CacheKeys.ProductsPattern)]
    [EnsureUserIsValid(new[] { "productId" })]
    public async Task<BaseControllerResponse<ProductDto>> UpdateProductAsync(Guid productId, UpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new NotFoundException("ProductNotFound", productId);

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        product.CategoryId = request.CategoryId;
        product.ProductType = request.ProductType;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.IsStockTracked = request.IsStockTracked;
        product.Sku = request.Sku;
        product.Status = request.Status;
        product.UpdatedAt = DateTime.UtcNow;
        product.UpdatedBy = userId;

        await _productRepository.UpdateAsync(product);

        var lang = GetLanguageCode();
        await _translationRepository.UpsertAsync(new ProductTranslationEntity
        {
            ProductId = product.Id,
            LanguageCode = lang,
            Name = request.Name,
            Description = request.Description
        });

        var dto = MapToDto(product, request.Name, request.Description);
        return ControllerResponseBuilder.Success(dto, "ProductUpdatedSuccessfully");
    }

    [HasPermission(Permissions.Products.HardDelete)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> HardDeleteProductAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            throw new NotFoundException("ProductNotFound", id);

        await _productRepository.DeleteAsync(id);
        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.Products.Update)]
    [ValidationAspect(typeof(Validators.FluentValidation.Products.AddImageRequestValidator))]
    public async Task<BaseControllerResponse<Guid>> AddProductImageAsync(AddImageRequest request)
    {
        var id = await _productRepository.AddProductImageAsync(request.ProductId, request.ImageUrl, request.AltText, request.SortOrder, request.IsPrimary);
        return ControllerResponseBuilder.Success(id);
    }

    [HasPermission(Permissions.Products.Update)]
    [ValidationAspect(typeof(Validators.FluentValidation.Products.UpsertAttributeRequestValidator))]
    public async Task<BaseControllerResponse> UpsertProductAttributeAsync(UpsertAttributeRequest request)
    {
        await _productRepository.UpsertProductAttributeAsync(request.ProductId, request.AttributeCode, request.AttributeName, request.ValueText);
        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.Products.Update)]
    [ValidationAspect(typeof(Validators.FluentValidation.Products.CreateVariantRequestValidator))]
    public async Task<BaseControllerResponse<Guid>> CreateVariantAsync(CreateVariantRequest request)
    {
        var id = await _productRepository.CreateVariantAsync(request.ProductId, request.Sku, request.Price, request.StockQuantity);
        return ControllerResponseBuilder.Success(id);
    }

    [HasPermission(Permissions.Products.Update)]
    [ValidationAspect(typeof(Validators.FluentValidation.Products.SetVariantAttributeRequestValidator))]
    public async Task<BaseControllerResponse> SetVariantAttributeAsync(SetVariantAttributeRequest request)
    {
        await _productRepository.SetVariantAttributeAsync(request.VariantId, request.AttributeCode, request.ValueText);
        return ControllerResponseBuilder.Success();
    }

    private ProductDto MapToDto(ProductLocalizedJoinEntity p) => new()
    {
        Id = p.Id,
        VendorId = p.VendorId,
        Name = p.Name,
        Description = p.Description,
        CategoryId = p.CategoryId,
        ProductType = p.ProductType,
        Price = p.Price,
        Unit = p.Unit,
        Barcode = p.Barcode,
        StockQuantity = p.StockQuantity,
        IsStockTracked = p.IsStockTracked,
        Sku = p.Sku,
        IsDecimalQuantityAllowed = p.IsDecimalQuantityAllowed,
        Status = p.Status,
        IsDeleted = p.IsDeleted,
        CreatedAt = p.CreatedAt,
        CreatedBy = p.CreatedBy,
        UpdatedAt = p.UpdatedAt,
        UpdatedBy = p.UpdatedBy,
        DeletedAt = p.DeletedAt,
        DeletedBy = p.DeletedBy,
        DeleteReason = p.DeleteReason
    };

    private ProductDto MapToDto(ProductEntity product, string name, string? description) => new()
    {
        Id = product.Id,
        VendorId = product.VendorId,
        Name = name,
        Description = description,
        CategoryId = product.CategoryId,
        ProductType = product.ProductType,
        Price = product.Price,
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

    private string GetLanguageCode()
        => _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString()?.Split(',').FirstOrDefault()?.ToLower() ?? "en";
}
