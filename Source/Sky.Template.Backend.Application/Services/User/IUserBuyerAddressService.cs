using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.BuyerAddresses;
using Sky.Template.Backend.Contract.Responses.BuyerAddressResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.User;

public interface IUserBuyerAddressService
{
    [HasPermission(Permissions.BuyerAddresses.View)]
    Task<BaseControllerResponse<PaginatedData<BuyerAddressResponse>>> GetFilteredPaginatedAsync(BuyerAddressFilterRequest request);
    [HasPermission(Permissions.BuyerAddresses.View)]
    Task<BaseControllerResponse<BuyerAddressResponse>> GetByIdAsync(Guid id);
    [HasPermission(Permissions.BuyerAddresses.View)]
    Task<BaseControllerResponse<IEnumerable<BuyerAddressResponse>>> GetByBuyerIdAsync(Guid buyerId);
    [HasPermission(Permissions.BuyerAddresses.Create)]
    Task<BaseControllerResponse<BuyerAddressResponse>> CreateAsync(CreateBuyerAddressRequest request);
    [HasPermission(Permissions.BuyerAddresses.Update)]
    Task<BaseControllerResponse<BuyerAddressResponse>> UpdateAsync(Guid id, UpdateBuyerAddressRequest request);
    [HasPermission(Permissions.BuyerAddresses.Delete)]
    Task<BaseControllerResponse> SoftDeleteAsync(Guid id);
}

public class UserBuyerAddressService : IUserBuyerAddressService
{
    private readonly IBuyerAddressRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserBuyerAddressService(IBuyerAddressRepository repository, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    [HasPermission(Permissions.BuyerAddresses.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.BuyerAddressesPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<PaginatedData<BuyerAddressResponse>>> GetFilteredPaginatedAsync(BuyerAddressFilterRequest request)
    {
        var (entities, totalCount) = await _repository.GetFilteredPaginatedAsync(request);
        var list = entities.Select(Map).ToList();
        return ControllerResponseBuilder.Success(new PaginatedData<BuyerAddressResponse>
        {
            Items = list,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
        });
    }

    [HasPermission(Permissions.BuyerAddresses.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.BuyerAddressesPrefix), ExpirationInMinutes = 60)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<BuyerAddressResponse>> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("BuyerAddressNotFound", id);
        return ControllerResponseBuilder.Success(Map(entity));
    }

    [HasPermission(Permissions.BuyerAddresses.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.BuyerAddressesPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<IEnumerable<BuyerAddressResponse>>> GetByBuyerIdAsync(Guid buyerId)
    {
        var entities = await _repository.GetByBuyerIdAsync(buyerId);
        var list = entities.Select(Map);
        return ControllerResponseBuilder.Success(list);
    }

    [HasPermission(Permissions.BuyerAddresses.Create)]
    [CacheRemove(nameof(CacheKeys.BuyerAddressesPattern))]
    [ValidationAspect(typeof(Validators.FluentValidation.BuyerAddresses.CreateBuyerAddressRequestValidator))]
    public async Task<BaseControllerResponse<BuyerAddressResponse>> CreateAsync(CreateBuyerAddressRequest request)
    {
        if (request.IsDefault && await _repository.HasDefaultAsync(request.BuyerId))
            throw new BusinessRulesException("DefaultAddressAlreadyExists");
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var entity = new BuyerAddressEntity
        {
            BuyerId = request.BuyerId,
            AddressTitle = request.AddressTitle,
            FullAddress = request.FullAddress,
            City = request.City,
            PostalCode = request.PostalCode,
            Country = request.Country,
            IsDefault = request.IsDefault,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };
        if (request.IsDefault)
            await _repository.ClearDefaultAsync(request.BuyerId);
        var created = await _repository.CreateAsync(entity);
        return ControllerResponseBuilder.Success(Map(created), "Created", HttpStatusCode.Created);
    }

    [HasPermission(Permissions.BuyerAddresses.Update)]
    [CacheRemove(nameof(CacheKeys.BuyerAddressesPattern))]
    [ValidationAspect(typeof(Validators.FluentValidation.BuyerAddresses.UpdateBuyerAddressRequestValidator))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<BuyerAddressResponse>> UpdateAsync(Guid id, UpdateBuyerAddressRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("BuyerAddressNotFound", id);
        if (request.IsDefault && await _repository.HasDefaultAsync(entity.BuyerId, id))
            throw new BusinessRulesException("DefaultAddressAlreadyExists");
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        entity.AddressTitle = request.AddressTitle;
        entity.FullAddress = request.FullAddress;
        entity.City = request.City;
        entity.PostalCode = request.PostalCode;
        entity.Country = request.Country;
        if (request.IsDefault)
        {
            await _repository.ClearDefaultAsync(entity.BuyerId);
            entity.IsDefault = true;
        }
        else
        {
            entity.IsDefault = request.IsDefault;
        }
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;
        var updated = await _repository.UpdateAsync(entity);
        return ControllerResponseBuilder.Success(Map(updated));
    }

    [HasPermission(Permissions.BuyerAddresses.Delete)]
    [CacheRemove(nameof(CacheKeys.BuyerAddressesPattern))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> SoftDeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("BuyerAddressNotFound", id);
        await _repository.SoftDeleteAsync(id);
        return ControllerResponseBuilder.Success();
    }

    private static BuyerAddressResponse Map(BuyerAddressEntity entity) => new()
    {
        Id = entity.Id,
        BuyerId = entity.BuyerId,
        AddressTitle = entity.AddressTitle,
        FullAddress = entity.FullAddress,
        City = entity.City,
        PostalCode = entity.PostalCode,
        Country = entity.Country,
        IsDefault = entity.IsDefault,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy
    };
}
