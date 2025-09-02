using Sky.Template.Backend.Core.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.Buyers;
using Sky.Template.Backend.Contract.Responses.BuyerResponses;
using Sky.Template.Backend.Contract.Responses.SaleResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.User;

public interface IUserBuyerService
{
    Task<BaseControllerResponse<IEnumerable<BuyerDto>>> GetMyBuyersAsync();
    Task<BaseControllerResponse<BuyerDto>> CreateBuyerAsync(CreateBuyerRequest request);
    Task<BaseControllerResponse<BuyerDto>> UpdateBuyerAsync(Guid buyerId, UpdateBuyerRequest request);
    Task<BaseControllerResponse> DeleteBuyerAsync(Guid buyerId);
    Task<BaseControllerResponse<SingleBuyerResponse>> GetBuyerByIdAsync(Guid id);
}

public class UserBuyerService : IUserBuyerService
{
    private readonly IBuyerRepository _buyerRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserBuyerService(IBuyerRepository buyerRepository, IHttpContextAccessor httpContextAccessor)
    {
        _buyerRepository = buyerRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.BuyersPrefix), ExpirationInMinutes = 60)]
    [HasPermission(Permissions.Buyers.Read)]
    public async Task<BaseControllerResponse<IEnumerable<BuyerDto>>> GetMyBuyersAsync()
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var buyers = await _buyerRepository.GetAllAsync();
        var list = buyers.Where(b => b.LinkedUserId == userId).Select(Map);
        return ControllerResponseBuilder.Success(list);
    }

    [CacheRemove(nameof(CacheKeys.BuyersPattern))]
    [ValidationAspect(typeof(Validators.FluentValidation.Buyers.CreateBuyerRequestValidator))]
    [HasPermission(Permissions.Buyers.Create)]
    public async Task<BaseControllerResponse<BuyerDto>> CreateBuyerAsync(CreateBuyerRequest request)
    {
        if (!await _buyerRepository.IsEmailUniqueAsync(request.Email ?? string.Empty))
            throw new BusinessRulesException("Buyer.EmailAlreadyExists");
        if (!await _buyerRepository.IsPhoneUniqueAsync(request.Phone ?? string.Empty))
            throw new BusinessRulesException("Buyer.PhoneAlreadyExists");

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var entity = new BuyerEntity
        {
            BuyerType = request.BuyerType,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            CompanyName = request.CompanyName,
            TaxNumber = request.TaxNumber,
            TaxOffice = request.TaxOffice,
            Description = request.Description,
            LinkedUserId = userId,
            CreatedBy = userId
        };
        var created = await _buyerRepository.CreateAsync(entity);
        return ControllerResponseBuilder.Success(Map(created));
    }

    [CacheRemove(nameof(CacheKeys.BuyersPattern))]
    [ValidationAspect(typeof(Validators.FluentValidation.Buyers.UpdateBuyerRequestValidator))]
    [HasPermission(Permissions.Buyers.Update)]
    public async Task<BaseControllerResponse<BuyerDto>> UpdateBuyerAsync(Guid buyerId, UpdateBuyerRequest request)
    {
        var entity = await _buyerRepository.GetByIdAsync(buyerId);
        if (entity == null)
            throw new NotFoundException("BuyerNotFound", buyerId);

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        if (entity.LinkedUserId != userId)
            throw new AuthorizationException("BuyerUnauthorized");

        entity.Name = request.Name;
        entity.Email = request.Email;
        entity.Phone = request.Phone;
        entity.CompanyName = request.CompanyName;
        entity.TaxNumber = request.TaxNumber;
        entity.TaxOffice = request.TaxOffice;
        entity.Description = request.Description;
        entity.UpdatedBy = userId;

        var updated = await _buyerRepository.UpdateAsync(entity);
        return ControllerResponseBuilder.Success(Map(updated));
    }

    [CacheRemove(nameof(CacheKeys.BuyersPattern))]
    [HasPermission(Permissions.Buyers.Delete)]
    public async Task<BaseControllerResponse> DeleteBuyerAsync(Guid buyerId)
    {
        var entity = await _buyerRepository.GetByIdAsync(buyerId);
        if (entity == null)
            throw new NotFoundException("BuyerNotFound", buyerId);

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        if (entity.LinkedUserId != userId)
            throw new AuthorizationException("BuyerUnauthorized");

        await _buyerRepository.SoftDeleteAsync(buyerId);
        return ControllerResponseBuilder.Success();
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.BuyersPrefix), ExpirationInMinutes = 60)]
    [HasPermission(Permissions.Buyers.Read)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<SingleBuyerResponse>> GetBuyerByIdAsync(Guid id)
    {
        var buyer = await _buyerRepository.GetByIdAsync(id);
        if (buyer == null)
            throw new NotFoundException("BuyerNotFound", id);
        var response = new SingleBuyerResponse
        {
            Id = buyer.Id,
            BuyerType = buyer.BuyerType,
            Name = buyer.Name,
            Email = buyer.Email,
            Phone = buyer.Phone,
            CompanyName = buyer.CompanyName,
            TaxNumber = buyer.TaxNumber,
            TaxOffice = buyer.TaxOffice,
            Description = buyer.Description,
            LinkedUserId = buyer.LinkedUserId,
            IsDeleted = buyer.IsDeleted,
            CreatedAt = buyer.CreatedAt,
            CreatedBy = buyer.CreatedBy,
            UpdatedAt = buyer.UpdatedAt,
            UpdatedBy = buyer.UpdatedBy,
            DeletedAt = buyer.DeletedAt,
            DeletedBy = buyer.DeletedBy,
            DeleteReason = buyer.DeleteReason
        };
        return ControllerResponseBuilder.Success(response);
    }

    private static BuyerDto Map(BuyerEntity buyer) => new()
    {
        Id = buyer.Id,
        BuyerType = buyer.BuyerType,
        Name = buyer.Name,
        Email = buyer.Email,
        Phone = buyer.Phone,
        CompanyName = buyer.CompanyName,
        TaxNumber = buyer.TaxNumber,
        TaxOffice = buyer.TaxOffice,
        Description = buyer.Description,
        LinkedUserId = buyer.LinkedUserId,
        CreatedAt = buyer.CreatedAt,
        CreatedBy = buyer.CreatedBy,
        UpdatedAt = buyer.UpdatedAt,
        UpdatedBy = buyer.UpdatedBy,
        IsDeleted = buyer.IsDeleted,
        DeletedAt = buyer.DeletedAt,
        DeletedBy = buyer.DeletedBy,
        DeleteReason = buyer.DeleteReason
    };
}
