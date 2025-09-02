using System.Net;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Sky.Template.Backend.Contract.Requests.CartItems;
using Sky.Template.Backend.Contract.Responses.CartItemResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.User;

public interface IUserCartItemService
{
    [HasPermission(Permissions.CartItems.View)]
    Task<BaseControllerResponse<IEnumerable<CartItemResponse>>> GetByCartIdAsync(Guid cartId);
    [HasPermission(Permissions.CartItems.Create)]
    Task<BaseControllerResponse<CartItemResponse>> CreateAsync(CreateCartItemRequest request);
    [HasPermission(Permissions.CartItems.Update)]
    Task<BaseControllerResponse<CartItemResponse>> UpdateAsync(Guid id, UpdateCartItemRequest request);
    [HasPermission(Permissions.CartItems.Delete)]
    Task<BaseControllerResponse> DeleteAsync(Guid id);
}

public class UserCartItemService : IUserCartItemService
{
    private readonly ICartItemRepository _repository;
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserCartItemService(
        ICartItemRepository repository,
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    [HasPermission(Permissions.CartItems.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.CartItemsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<IEnumerable<CartItemResponse>>> GetByCartIdAsync(Guid cartId)
    {
        var lang = _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString()?.Split(',').FirstOrDefault()?.ToLower() ?? "en";
        var entities = await _repository.GetByCartIdAsync(cartId, lang);
        var list = entities.Select(Map);
        return ControllerResponseBuilder.Success(list);
    }

    [HasPermission(Permissions.CartItems.Create)]
    [CacheRemove(nameof(CacheKeys.CartItemsPattern))]
    [ValidationAspect(typeof(Validators.FluentValidation.CartItems.CreateCartItemRequestValidator))]
    public async Task<BaseControllerResponse<CartItemResponse>> CreateAsync(CreateCartItemRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var cart = await _cartRepository.GetByIdAsync(request.CartId) ?? throw new NotFoundException("CartNotFound", request.CartId);
        if (cart.Status != "OPEN")
            throw new BusinessRulesException("CartNotActive");
        var product = await _productRepository.GetByIdAsync(request.ProductId) ?? throw new NotFoundException("ProductNotFound", request.ProductId);
        var unitPrice = request.UnitPrice ?? product.Price;
        var currency = request.Currency ?? cart.Currency;
        var existing = await _repository.GetByCartAndProductAsync(request.CartId, request.ProductId);
        CartItemEntity entity;
        if (existing != null)
        {
            existing.Quantity += request.Quantity;
            existing.UnitPrice = unitPrice;
            existing.Currency = currency;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = userId;
            entity = await _repository.UpdateAsync(existing);
        }
        else
        {
            entity = new CartItemEntity
            {
                CartId = request.CartId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                UnitPrice = unitPrice,
                Currency = currency,
                Status = "ACTIVE",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                IsDeleted = false
            };
            entity = await _repository.CreateAsync(entity);
        }
        await UpdateCartTotalAsync(request.CartId);
        var lang = _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString()?.Split(',').FirstOrDefault()?.ToLower() ?? "en";
        var localized = await _repository.GetByIdWithProductAsync(entity.Id, lang);
        return ControllerResponseBuilder.Success(Map(localized!), "Created", HttpStatusCode.Created);
    }

    [HasPermission(Permissions.CartItems.Update)]
    [CacheRemove(nameof(CacheKeys.CartItemsPattern))]
    [ValidationAspect(typeof(Validators.FluentValidation.CartItems.UpdateCartItemRequestValidator))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<CartItemResponse>> UpdateAsync(Guid id, UpdateCartItemRequest request)
    {
        var entity = await _repository.GetByIdAsync(id) ?? throw new NotFoundException("CartItemNotFound", id);
        var cart = await _cartRepository.GetByIdAsync(entity.CartId) ?? throw new NotFoundException("CartNotFound", entity.CartId);
        if (cart.Status != "OPEN")
            throw new BusinessRulesException("CartNotActive");
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        if (request.ProductId.HasValue && request.ProductId.Value != entity.ProductId)
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId.Value) ?? throw new NotFoundException("ProductNotFound", request.ProductId.Value);
            var exists = await _repository.GetByCartAndProductAsync(entity.CartId, request.ProductId.Value);
            if (exists != null && exists.Id != id)
                throw new BusinessRulesException("ProductAlreadyInCart");
            entity.ProductId = request.ProductId.Value;
            if (!request.UnitPrice.HasValue)
                entity.UnitPrice = product.Price;
        }
        if (request.Quantity.HasValue)
            entity.Quantity = request.Quantity.Value;
        if (request.UnitPrice.HasValue)
            entity.UnitPrice = request.UnitPrice.Value;
        if (!string.IsNullOrEmpty(request.Currency))
            entity.Currency = request.Currency!;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;
        var updated = await _repository.UpdateAsync(entity);
        await UpdateCartTotalAsync(entity.CartId);
        var lang = _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString()?.Split(',').FirstOrDefault()?.ToLower() ?? "en";
        var localized = await _repository.GetByIdWithProductAsync(updated.Id, lang);
        return ControllerResponseBuilder.Success(Map(localized!));
    }

    [HasPermission(Permissions.CartItems.Delete)]
    [CacheRemove(nameof(CacheKeys.CartItemsPattern))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id) ?? throw new NotFoundException("CartItemNotFound", id);
        await _repository.SoftDeleteAsync(id);
        await UpdateCartTotalAsync(entity.CartId);
        return ControllerResponseBuilder.Success();
    }

    private async Task UpdateCartTotalAsync(Guid cartId)
    {
        var total = await _cartRepository.CalculateTotalPriceAsync(cartId);
        var cart = await _cartRepository.GetByIdAsync(cartId);
        if (cart != null)
        {
            cart.TotalPrice = total;
            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.UpdateAsync(cart);
        }
    }

    private static CartItemResponse Map(CartItemLocalizedJoinEntity entity) => new()
    {
        Id = entity.Id,
        CartId = entity.CartId,
        ProductId = entity.ProductId,
        ProductName = entity.ProductName,
        Quantity = entity.Quantity,
        UnitPrice = entity.UnitPrice,
        Subtotal = entity.UnitPrice * entity.Quantity,
        Currency = entity.Currency,
        Status = entity.Status,
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy,
        DeletedAt = entity.DeletedAt,
        DeletedBy = entity.DeletedBy,
        DeleteReason = entity.DeleteReason
    };
}
