using Sky.Template.Backend.Core.Constants;
using System.Net;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Application.Validators.FluentValidation.DiscountUsages;
using Sky.Template.Backend.Contract.Requests.DiscountUsages;
using Sky.Template.Backend.Contract.Responses.DiscountUsageResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminDiscountUsageService
{
    Task<BaseControllerResponse<DiscountUsageListResponse>> GetDiscountUsagesAsync(DiscountUsageFilterRequest request);
    Task<BaseControllerResponse<DiscountUsageResponse>> CreateDiscountUsageAsync(CreateDiscountUsageRequest request);
}

public class AdminDiscountUsageService : IAdminDiscountUsageService
{
    private readonly IDiscountUsageRepository _discountUsageRepository;
    private readonly IDiscountRepository _discountRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdminDiscountUsageService(IDiscountUsageRepository discountUsageRepository, IDiscountRepository discountRepository, IHttpContextAccessor httpContextAccessor)
    {
        _discountUsageRepository = discountUsageRepository;
        _discountRepository = discountRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    [HasPermission(Permissions.DiscountUsages.View)]
    public async Task<BaseControllerResponse<DiscountUsageListResponse>> GetDiscountUsagesAsync(DiscountUsageFilterRequest request)
    {
        var (usages, total) = await _discountUsageRepository.GetFilteredPaginatedAsync(request);
        var items = usages.Select(u => new DiscountUsageDto
        {
            Id = u.Id,
            DiscountId = u.DiscountId,
            BuyerId = u.BuyerId,
            OrderId = u.OrderId,
            UsedAt = u.UsedAt,
            CreatedAt = u.CreatedAt,
            CreatedBy = u.CreatedBy
        }).ToList();
        return ControllerResponseBuilder.Success(new DiscountUsageListResponse
        {
            DiscountUsages = new Core.BaseResponse.PaginatedData<DiscountUsageDto>
            {
                Items = items,
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPage = (int)Math.Ceiling((double)total / request.PageSize)
            }
        });
    }

    [HasPermission(Permissions.DiscountUsages.Create)]
    [ValidationAspect(typeof(CreateDiscountUsageRequestValidator))]
    public async Task<BaseControllerResponse<DiscountUsageResponse>> CreateDiscountUsageAsync(CreateDiscountUsageRequest request)
    {
        var discount = await _discountRepository.GetByIdAsync(request.DiscountId);
        if (discount == null)
            throw new NotFoundException("DiscountNotFound", request.DiscountId);

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var entity = new DiscountUsageEntity
        {
            DiscountId = request.DiscountId,
            BuyerId = request.BuyerId,
            OrderId = request.OrderId,
            UsedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        var created = await _discountUsageRepository.CreateAsync(entity);
        var dto = new DiscountUsageDto
        {
            Id = created.Id,
            DiscountId = created.DiscountId,
            BuyerId = created.BuyerId,
            OrderId = created.OrderId,
            UsedAt = created.UsedAt,
            CreatedAt = created.CreatedAt,
            CreatedBy = created.CreatedBy
        };
        return ControllerResponseBuilder.Success(new DiscountUsageResponse { DiscountUsage = dto }, "DiscountUsageCreated", HttpStatusCode.Created);
    }
}
