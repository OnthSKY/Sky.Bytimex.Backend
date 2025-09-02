using Sky.Template.Backend.Core.Constants;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.Products;
using Sky.Template.Backend.Contract.Responses.ProductResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Application.Services.System;
using System.ComponentModel.DataAnnotations;
using Sky.Template.Backend.Infrastructure.Entities.Product;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;
using System.Linq;

namespace Sky.Template.Backend.Application.Services.Vendor;

public interface IVendorProductService
{
    Task<BaseControllerResponse<SingleProductResponse>> CreateProductAsync(CreateProductRequest request);
    Task<BaseControllerResponse<SingleProductResponse>> UpdateProductAsync(Guid id, UpdateProductRequest request);
    Task<BaseControllerResponse> SoftDeleteProductAsync(Guid id);
    Task<BaseControllerResponse<ProductListPaginatedDtoResponse>> GetFilteredPaginatedProductsAsync(ProductFilterRequest request);
    Task<BaseControllerResponse<ProductStockDto>> GetProductStockStatusAsync(Guid productId);
    Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeId = null);
}

public class VendorProductService : IVendorProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IProductTranslationRepository _translationRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISettingsService _settingsService;
    private readonly IProductSettingsService _productSettingsService;
    private readonly IVendorRepository _vendorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VendorProductService(IProductRepository productRepository,
        IProductTranslationRepository translationRepository,
        IVendorRepository vendorRepository,
        IHttpContextAccessor httpContextAccessor,
        ISettingsService settingsService,
        IProductSettingsService productSettingsService,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _translationRepository = translationRepository;
        _vendorRepository = vendorRepository;
        _httpContextAccessor = httpContextAccessor;
        _settingsService = settingsService;
        _productSettingsService = productSettingsService;
        _unitOfWork = unitOfWork;
    }

    [HasPermission(Permissions.Products.Create)]
    [ValidationAspect(typeof(Validators.FluentValidation.Products.CreateProductRequestValidator))]
    public async Task<BaseControllerResponse<SingleProductResponse>> CreateProductAsync(CreateProductRequest request)
    {
        if (!await IsSkuUniqueAsync(request.Sku))
            throw new BusinessRulesException("Product.SkuAlreadyExists");

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        SingleProductResponse? response = null;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var settings = await _settingsService.GetSettingsAsync(new[] { "MAINTENANCE_MODE", "MAX_PRODUCT_COUNT_PER_VENDOR", "REQUIRE_VENDOR_KYC_FOR_PUBLISHING" });

            if (settings.TryGetValue("MAINTENANCE_MODE", out var maintenanceStr) &&
                bool.TryParse(maintenanceStr, out var maintenance) &&
                maintenance)
            {
                throw new MaintenanceModeException();
            }

            if (!settings.TryGetValue("MAX_PRODUCT_COUNT_PER_VENDOR", out var maxCountStr) ||
                !int.TryParse(maxCountStr, out var maxCount) ||
                maxCount < 0)
            {
                throw new ValidationException("InvalidMaxProductCount");
            }

            var activeCount = await GetActiveProductCountAsync(userId);
            if (activeCount >= maxCount)
                throw new MaxProductLimitExceededException();

            if (settings.TryGetValue("REQUIRE_VENDOR_KYC_FOR_PUBLISHING", out var requireKycStr) &&
                bool.TryParse(requireKycStr, out var requireKyc) &&
                requireKyc)
            {
                var kycStatus = await GetVendorKycStatusAsync(userId);
                if (!string.Equals(kycStatus, "VERIFIED", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(request.Status, "PUBLISHED", StringComparison.OrdinalIgnoreCase))
                        throw new KycRequiredException();

                    request.Status = "DRAFT";
                }
            }


            var product = new ProductEntity
            {
                Id = Guid.NewGuid(),
                VendorId = userId,
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

            var lang = GetLanguageCode();
            await InsertProductAsync(product, request.Name, request.Description, lang);
            response = MapToResponse(product, request.Name, request.Description);

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        return ControllerResponseBuilder.Success(response!, "ProductCreatedSuccessfully");
    }

    protected virtual Task<int> GetActiveProductCountAsync(Guid vendorId)
        => _productRepository.GetActiveProductCountAsync(vendorId, _unitOfWork.Connection, _unitOfWork.Transaction);

    protected virtual Task<string?> GetVendorKycStatusAsync(Guid vendorId)
        => _vendorRepository.GetKycStatusAsync(vendorId, _unitOfWork.Connection, _unitOfWork.Transaction);

    protected virtual async Task InsertProductAsync(ProductEntity product, string name, string? description, string lang)
    {
        await _productRepository.InsertAsync(product, _unitOfWork.Connection, _unitOfWork.Transaction);
        await _translationRepository.UpsertAsync(new ProductTranslationEntity
        {
            ProductId = product.Id,
            LanguageCode = lang,
            Name = name,
            Description = description
        }, _unitOfWork.Connection, _unitOfWork.Transaction);
    }

    [HasPermission(Permissions.Products.Update)]
    [ValidationAspect(typeof(Validators.FluentValidation.Products.UpdateProductRequestValidator))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<SingleProductResponse>> UpdateProductAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            throw new NotFoundException("ProductNotFound", id);
        var userId = _httpContextAccessor.HttpContext.GetUserId();

        var settings = await _productSettingsService.GetEffectiveSettingsAsync(userId);
        if (settings.MaintenanceMode)
            throw new MaintenanceModeException();

        if (settings.RequireVendorKycForPublishing)
        {
            var kycStatus = await GetVendorKycStatusAsync(userId);
            if (!string.Equals(kycStatus, "VERIFIED", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(request.Status, "PUBLISHED", StringComparison.OrdinalIgnoreCase))
                    throw new KycRequiredException();
                request.Status = "DRAFT";
            }
        }

        product.CategoryId = request.CategoryId;
        product.ProductType = request.ProductType;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.IsStockTracked = request.IsStockTracked;
        product.Sku = request.Sku;
        product.Status = request.Status;
        product.UpdatedAt = DateTime.UtcNow;
        product.UpdatedBy = userId;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _productRepository.UpdateAsync(product, _unitOfWork.Connection, _unitOfWork.Transaction);
            var lang = GetLanguageCode();
            await _translationRepository.UpsertAsync(new ProductTranslationEntity
            {
                ProductId = product.Id,
                LanguageCode = lang,
                Name = request.Name,
                Description = request.Description
            }, _unitOfWork.Connection, _unitOfWork.Transaction);

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        var response = MapToResponse(product, request.Name, request.Description);
        return ControllerResponseBuilder.Success(response, "ProductUpdatedSuccessfully");
    }

    [HasPermission(Permissions.Products.Delete)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> SoftDeleteProductAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            throw new NotFoundException("ProductNotFound", id);
        var userId = _httpContextAccessor.HttpContext.GetUserId();

        var settings = await _productSettingsService.GetEffectiveSettingsAsync(userId);
        if (settings.MaintenanceMode)
            throw new MaintenanceModeException();
        if (!settings.AllowProductDeletion)
            throw new BusinessRulesException("ProductDeletionNotAllowed");

        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
        product.DeletedBy = userId;

        await _productRepository.UpdateAsync(product);
        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.Products.Read)]
    public async Task<BaseControllerResponse<ProductListPaginatedDtoResponse>> GetFilteredPaginatedProductsAsync(ProductFilterRequest request)
    {
        var vendorId = _httpContextAccessor.HttpContext.GetUserId();
        request.Filters["vendorId"] = vendorId.ToString();

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

        return ControllerResponseBuilder.Success(new ProductListPaginatedDtoResponse { Products = paginated });
    }

    [HasPermission(Permissions.Products.Read)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.StockStatusPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<ProductStockDto>> GetProductStockStatusAsync(Guid productId)
    {
        var vendorId = _httpContextAccessor.HttpContext.GetUserId();
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null || (product.CreatedBy != vendorId && product.VendorId != vendorId))
            throw new NotFoundException("ProductNotFound", productId);

        var dto = new ProductStockDto
        {
            ProductId = product.Id,
            StockQuantity = product.StockQuantity ?? 0
        };

        return ControllerResponseBuilder.Success(dto);
    }

    public Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeId = null)
        => _productRepository.IsSkuUniqueAsync(sku, excludeId);

    private static ProductDto MapToDto(ProductLocalizedJoinEntity p) => new()
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

    private static SingleProductResponse MapToResponse(ProductEntity product, string name, string? description) => new()
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
