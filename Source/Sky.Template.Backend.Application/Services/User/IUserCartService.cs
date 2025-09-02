using System.Net;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.Carts;
using Sky.Template.Backend.Contract.Responses.CartResponses;
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
using System.Linq;

namespace Sky.Template.Backend.Application.Services.User;

public interface IUserCartService
{
    [HasPermission(Permissions.Carts.View)]
    Task<BaseControllerResponse<CartListPaginatedResponse>> GetFilteredPaginatedAsync(CartFilterRequest request);
    [HasPermission(Permissions.Carts.View)]
    Task<BaseControllerResponse<CartResponse>> GetByIdAsync(Guid id);
    [HasPermission(Permissions.Carts.Create)]
    Task<BaseControllerResponse<CartResponse>> CreateAsync(CreateCartRequest request);
    [HasPermission(Permissions.Carts.Update)]
    Task<BaseControllerResponse<CartResponse>> UpdateAsync(Guid id, UpdateCartRequest request);
    [HasPermission(Permissions.Carts.Delete)]
    Task<BaseControllerResponse> DeleteAsync(Guid id);
}

public class UserCartService : IUserCartService
{
    private readonly ICartRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserCartService(ICartRepository repository, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    [HasPermission(Permissions.Carts.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.CartsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<CartListPaginatedResponse>> GetFilteredPaginatedAsync(CartFilterRequest request)
    {
        var (entities, totalCount) = await _repository.GetFilteredPaginatedAsync(request);
        var list = entities.Select(Map).ToList();
        var paginated = new PaginatedData<CartResponse>
        {
            Items = list,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
        return ControllerResponseBuilder.Success(new CartListPaginatedResponse { Carts = paginated });
    }

    [HasPermission(Permissions.Carts.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.CartsPrefix), ExpirationInMinutes = 60)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<CartResponse>> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("CartNotFound", id);
        return ControllerResponseBuilder.Success(Map(entity));
    }

    [HasPermission(Permissions.Carts.Create)]
    [CacheRemove(nameof(CacheKeys.CartsPattern))]
    [ValidationAspect(typeof(Validators.FluentValidation.Carts.CreateCartRequestValidator))]
    public async Task<BaseControllerResponse<CartResponse>> CreateAsync(CreateCartRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var entity = new CartEntity
        {
            BuyerId = request.BuyerId,
            CouponCode = request.CouponCode,
            Currency = request.Currency,
            Note = request.Note,
            Status = "OPEN",
            TotalPrice = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };
        var created = await _repository.CreateAsync(entity);
        return ControllerResponseBuilder.Success(Map(created), "Created", HttpStatusCode.Created);
    }

    [HasPermission(Permissions.Carts.Update)]
    [CacheRemove(nameof(CacheKeys.CartsPattern))]
    [ValidationAspect(typeof(Validators.FluentValidation.Carts.UpdateCartRequestValidator))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<CartResponse>> UpdateAsync(Guid id, UpdateCartRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("CartNotFound", id);
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        if (request.CouponCode != null)
            entity.CouponCode = request.CouponCode;
        if (request.Note != null)
            entity.Note = request.Note;
        entity.TotalPrice = await _repository.CalculateTotalPriceAsync(id);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;
        var updated = await _repository.UpdateAsync(entity);
        return ControllerResponseBuilder.Success(Map(updated));
    }

    [HasPermission(Permissions.Carts.Delete)]
    [CacheRemove(nameof(CacheKeys.CartsPattern))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("CartNotFound", id);
        await _repository.SoftDeleteAsync(id);
        return ControllerResponseBuilder.Success();
    }

    private static CartResponse Map(CartEntity entity) => new()
    {
        Id = entity.Id,
        BuyerId = entity.BuyerId,
        Status = entity.Status,
        CouponCode = entity.CouponCode,
        Currency = entity.Currency,
        TotalPrice = entity.TotalPrice,
        Note = entity.Note,
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
