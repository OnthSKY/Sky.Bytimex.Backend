using Sky.Template.Backend.Core.Constants;
using System.Net;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Application.Validators.FluentValidation.Discounts;
using Sky.Template.Backend.Contract.Requests.Discounts;
using Sky.Template.Backend.Contract.Responses.DiscountResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminDiscountService
{
    Task<BaseControllerResponse<DiscountListResponse>> GetDiscountsAsync(DiscountFilterRequest request);
    Task<BaseControllerResponse<DiscountResponse>> GetDiscountByIdAsync(Guid id);
    Task<BaseControllerResponse<DiscountResponse>> CreateDiscountAsync(CreateDiscountRequest request);
    Task<BaseControllerResponse<DiscountResponse>> UpdateDiscountAsync(UpdateDiscountRequest request);
    Task<BaseControllerResponse> DeleteDiscountAsync(Guid id);
}

public class AdminDiscountService : IAdminDiscountService
{
    private readonly IDiscountRepository _discountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const int CacheDuration = 60;

    public AdminDiscountService(IDiscountRepository discountRepository, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _discountRepository = discountRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    [HasPermission(Permissions.Discounts.View)]
    public async Task<BaseControllerResponse<DiscountListResponse>> GetDiscountsAsync(DiscountFilterRequest request)
    {
        var (discounts, totalCount) = await _discountRepository.GetFilteredPaginatedAsync(request);
        var dtoList = discounts.Select(MapToDto).ToList();
        return ControllerResponseBuilder.Success(new DiscountListResponse
        {
            Discounts = new Core.BaseResponse.PaginatedData<DiscountDto>
            {
                Items = dtoList,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
            }
        });
    }

    [HasPermission(Permissions.Discounts.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.DiscountsPrefix), ExpirationInMinutes = CacheDuration)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<DiscountResponse>> GetDiscountByIdAsync(Guid id)
    {
        var discount = await _discountRepository.GetByIdAsync(id);
        if (discount == null)
            throw new NotFoundException("DiscountNotFound", id);
        return ControllerResponseBuilder.Success(new DiscountResponse { Discount = MapToDto(discount) });
    }

    [HasPermission(Permissions.Discounts.Create)]
    [ValidationAspect(typeof(CreateDiscountRequestValidator))]
    [InvalidateCache(CacheKeys.DiscountsPattern)]
    public async Task<BaseControllerResponse<DiscountResponse>> CreateDiscountAsync(CreateDiscountRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var entity = new DiscountEntity
        {
            Code = request.Code,
            Description = request.Description,
            DiscountType = request.DiscountType,
            Value = request.Value,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            UsageLimit = request.UsageLimit,
            CreatedBy = userId
        };
        var created = await _discountRepository.CreateAsync(entity);
        await _unitOfWork.CommitAsync();
        return ControllerResponseBuilder.Success(new DiscountResponse { Discount = MapToDto(created) }, "DiscountCreated",  HttpStatusCode.Created);
    }

    [HasPermission(Permissions.Discounts.Update)]
    [ValidationAspect(typeof(UpdateDiscountRequestValidator))]
    [InvalidateCache(CacheKeys.DiscountsPattern)]
    public async Task<BaseControllerResponse<DiscountResponse>> UpdateDiscountAsync(UpdateDiscountRequest request)
    {
        var discount = await _discountRepository.GetByIdAsync(request.Id);
        if (discount == null)
            throw new NotFoundException("DiscountNotFound", request.Id);
        discount.Code = request.Code;
        discount.Description = request.Description;
        discount.DiscountType = request.DiscountType;
        discount.Value = request.Value;
        discount.StartDate = request.StartDate;
        discount.EndDate = request.EndDate;
        discount.UsageLimit = request.UsageLimit;
        discount.UpdatedBy = _httpContextAccessor.HttpContext.GetUserId();
        var updated = await _discountRepository.UpdateAsync(discount);
        await _unitOfWork.CommitAsync();
        return ControllerResponseBuilder.Success(new DiscountResponse { Discount = MapToDto(updated) }, "DiscountUpdated");
    }

    [HasPermission(Permissions.Discounts.Delete)]
    [InvalidateCache(CacheKeys.DiscountsPattern)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> DeleteDiscountAsync(Guid id)
    {
        var success = await _discountRepository.SoftDeleteAsync(id);
        if (!success)
            throw new NotFoundException("DiscountNotFound", id);
        await _unitOfWork.CommitAsync();
        return ControllerResponseBuilder.Success();
    }

    private static DiscountDto MapToDto(DiscountEntity d) => new()
    {
        Id = d.Id,
        Code = d.Code,
        Description = d.Description,
        DiscountType = d.DiscountType,
        Value = d.Value,
        StartDate = d.StartDate,
        EndDate = d.EndDate,
        UsageLimit = d.UsageLimit,
        CreatedAt = d.CreatedAt,
        CreatedBy = d.CreatedBy,
        UpdatedAt = d.UpdatedAt,
        UpdatedBy = d.UpdatedBy
    };
}
