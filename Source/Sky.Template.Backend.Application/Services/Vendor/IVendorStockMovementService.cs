using Sky.Template.Backend.Core.Constants;
using System.Net;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.StockMovements;
using Sky.Template.Backend.Contract.Responses.StockMovementResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities.Stock;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.Vendor;

public interface IVendorStockMovementService
{
    Task<BaseControllerResponse<StockMovementListResponse>> GetAsync(StockMovementFilterRequest request);
    Task<BaseControllerResponse<StockMovementResponse>> GetByIdAsync(Guid id);
    Task<BaseControllerResponse<StockMovementResponse>> CreateAsync(CreateStockMovementRequest request);
    Task<BaseControllerResponse<StockMovementResponse>> UpdateAsync(Guid id, UpdateStockMovementRequest request);
    Task<BaseControllerResponse> DeleteAsync(Guid id);
    Task<BaseControllerResponse<decimal>> GetProductStockAsync(Guid productId);
}

public class VendorStockMovementService : StockMovementServiceBase, IVendorStockMovementService
{
    public VendorStockMovementService(IStockMovementRepository movementRepository,
        IProductRepository productRepository,
        IHttpContextAccessor httpContextAccessor) : base(movementRepository, productRepository, httpContextAccessor)
    {
    }

    [HasPermission(Permissions.StockMovements.View)]
    public async Task<BaseControllerResponse<StockMovementListResponse>> GetAsync(StockMovementFilterRequest request)
    {
        var vendorId = GetUserId();
        request.Filters["supplierId"] = vendorId.ToString();
        var (data, totalCount) = await _movementRepository.GetFilteredPaginatedAsync(request);
        var mapped = data.Select(MapToResponse).ToList();
        var paginated = new PaginatedData<StockMovementResponse>
        {
            Items = mapped,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
        return ControllerResponseBuilder.Success(new StockMovementListResponse { StockMovements = paginated });
    }

    [HasPermission(Permissions.StockMovements.View)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<StockMovementResponse>> GetByIdAsync(Guid id)
    {
        var vendorId = GetUserId();
        var entity = await _movementRepository.GetByIdAsync(id);
        if (entity == null || entity.SupplierId != vendorId)
            throw new NotFoundException("StockMovementNotFound", id);
        return ControllerResponseBuilder.Success(MapToResponse(entity));
    }

    [HasPermission(Permissions.StockMovements.Create)]
    [ValidationAspect(typeof(Validators.FluentValidation.StockMovements.CreateStockMovementRequestValidator))]
    [CacheRemove(nameof(CacheKeys.StockStatusPattern))]
    public async Task<BaseControllerResponse<StockMovementResponse>> CreateAsync(CreateStockMovementRequest request)
    {
        var vendorId = GetUserId();
        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product == null || (product.CreatedBy != vendorId && product.VendorId != vendorId))
            throw new NotFoundException("ProductNotFound", request.ProductId);
        var movementDate = request.MovementDate ?? DateTime.UtcNow;
        var delta = CalculateDelta(request.MovementType, request.Quantity, product.StockQuantity ?? 0);
        product.StockQuantity += delta;
        var movement = new StockMovementEntity
        {
            ProductId = request.ProductId,
            SupplierId = vendorId,
            MovementType = request.MovementType,
            Quantity = request.Quantity,
            MovementDate = movementDate,
            Description = request.Description,
            RelatedOrderId = request.RelatedOrderId,
            Status = "ACTIVE",
            CreatedBy = vendorId,
            CreatedAt = DateTime.UtcNow
        };
        await _movementRepository.CreateAsync(movement);
        await _productRepository.UpdateAsync(product);
        return ControllerResponseBuilder.Success(MapToResponse(movement), "StockMovementCreated", HttpStatusCode.Created);
    }

    [HasPermission(Permissions.StockMovements.Update)]
    [ValidationAspect(typeof(Validators.FluentValidation.StockMovements.UpdateStockMovementRequestValidator))]
    [CacheRemove(nameof(CacheKeys.StockStatusPattern))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<StockMovementResponse>> UpdateAsync(Guid id, UpdateStockMovementRequest request)
    {
        var vendorId = GetUserId();
        var entity = await _movementRepository.GetByIdAsync(id);
        if (entity == null || entity.SupplierId != vendorId)
            throw new NotFoundException("StockMovementNotFound", id);
        var product = await _productRepository.GetByIdAsync(entity.ProductId);
        if (product == null)
            throw new NotFoundException("ProductNotFound", entity.ProductId);
        var oldDelta = CalculateDelta(entity.MovementType, entity.Quantity, product.StockQuantity ?? 0);
        product.StockQuantity -= oldDelta;
        var newProduct = await _productRepository.GetByIdAsync(request.ProductId);
        if (newProduct == null || (newProduct.CreatedBy != vendorId && newProduct.VendorId != vendorId))
            throw new NotFoundException("ProductNotFound", request.ProductId);
        var newDelta = CalculateDelta(request.MovementType, request.Quantity, newProduct.StockQuantity ?? 0);
        newProduct.StockQuantity += newDelta;
        entity.ProductId = request.ProductId;
        entity.MovementType = request.MovementType;
        entity.Quantity = request.Quantity;
        entity.MovementDate = request.MovementDate ?? entity.MovementDate;
        entity.Description = request.Description;
        entity.RelatedOrderId = request.RelatedOrderId;
        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = vendorId;
        var updated = await _movementRepository.UpdateAsync(entity);
        await _productRepository.UpdateAsync(product);
        if (newProduct.Id != product.Id)
            await _productRepository.UpdateAsync(newProduct);
        else
            product.StockQuantity = newProduct.StockQuantity;
        return ControllerResponseBuilder.Success(MapToResponse(updated), "StockMovementUpdated");
    }

    [HasPermission(Permissions.StockMovements.Delete)]
    [CacheRemove(nameof(CacheKeys.StockStatusPattern))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> DeleteAsync(Guid id)
    {
        var vendorId = GetUserId();
        var entity = await _movementRepository.GetByIdAsync(id);
        if (entity == null || entity.SupplierId != vendorId)
            throw new NotFoundException("StockMovementNotFound", id);
        var product = await _productRepository.GetByIdAsync(entity.ProductId);
        if (product == null)
            throw new NotFoundException("ProductNotFound", entity.ProductId);
        var delta = CalculateDelta(entity.MovementType, entity.Quantity, product.StockQuantity ?? 0);
        product.StockQuantity -= delta;
        await _productRepository.UpdateAsync(product);
        await _movementRepository.SoftDeleteAsync(id);
        return ControllerResponseBuilder.Success();
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.StockStatusPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<decimal>> GetProductStockAsync(Guid productId)
    {
        var vendorId = GetUserId();
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null || (product.CreatedBy != vendorId && product.VendorId != vendorId))
            throw new NotFoundException("ProductNotFound", productId);
        return ControllerResponseBuilder.Success(product.StockQuantity ?? 0);
    }
}
