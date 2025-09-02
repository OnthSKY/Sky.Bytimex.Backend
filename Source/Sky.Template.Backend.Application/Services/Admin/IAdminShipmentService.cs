using Microsoft.AspNetCore.Http;
using System.Linq;
using Sky.Template.Backend.Contract.Requests.Shipments;
using Sky.Template.Backend.Contract.Responses.ShipmentResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminShipmentService
{
    [HasPermission(Permissions.Shipments.View)]
    Task<BaseControllerResponse<ShipmentListPaginatedResponse>> GetFilteredPaginatedAsync(ShipmentFilterRequest request);
    [HasPermission(Permissions.Shipments.View)]
    Task<BaseControllerResponse<ShipmentResponse>> GetByIdAsync(Guid id);
    [HasPermission(Permissions.Shipments.View)]
    Task<BaseControllerResponse<IEnumerable<ShipmentResponse>>> GetByOrderIdAsync(Guid orderId);
    [HasPermission(Permissions.Shipments.View)]
    Task<BaseControllerResponse<IEnumerable<ShipmentResponse>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    [HasPermission(Permissions.Shipments.Create)]
    Task<BaseControllerResponse<ShipmentResponse>> CreateAsync(CreateShipmentRequest request);
    [HasPermission(Permissions.Shipments.Update)]
    Task<BaseControllerResponse<ShipmentResponse>> UpdateAsync(Guid id, UpdateShipmentRequest request);
    [HasPermission(Permissions.Shipments.Delete)]
    Task<BaseControllerResponse> DeleteAsync(Guid id);
}

[HasPermission(Permissions.Shipments.All)]
public class AdminShipmentService : ShipmentServiceBase, IAdminShipmentService
{
    public AdminShipmentService(IShipmentRepository repository, IHttpContextAccessor httpContextAccessor)
        : base(repository, httpContextAccessor) { }

    [HasPermission(Permissions.Shipments.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.ShipmentsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<ShipmentListPaginatedResponse>> GetFilteredPaginatedAsync(ShipmentFilterRequest request)
    {
        var (entities, totalCount) = await _repository.GetFilteredPaginatedAsync(request);
        var list = entities.Select(Map).ToList();
        var paginated = new PaginatedData<ShipmentResponse>
        {
            Items = list,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
        return ControllerResponseBuilder.Success(new ShipmentListPaginatedResponse { Shipments = paginated });
    }

    [HasPermission(Permissions.Shipments.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.ShipmentsPrefix), ExpirationInMinutes = 60)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<ShipmentResponse>> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("ShipmentNotFound", id);
        return ControllerResponseBuilder.Success(Map(entity));
    }

    [HasPermission(Permissions.Shipments.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.ShipmentsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<IEnumerable<ShipmentResponse>>> GetByOrderIdAsync(Guid orderId)
    {
        var entities = await _repository.GetByOrderIdAsync(orderId);
        var list = entities.Select(Map);
        return ControllerResponseBuilder.Success(list);
    }

    [HasPermission(Permissions.Shipments.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.ShipmentsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<IEnumerable<ShipmentResponse>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var entities = await _repository.GetByDateRangeAsync(startDate, endDate);
        var list = entities.Select(Map);
        return ControllerResponseBuilder.Success(list);
    }

    [HasPermission(Permissions.Shipments.Create)]
    [CacheRemove(nameof(CacheKeys.ShipmentsPattern))]
    [ValidationAspect(typeof(Validators.FluentValidation.Shipments.CreateShipmentRequestValidator))]
    public Task<BaseControllerResponse<ShipmentResponse>> CreateAsync(CreateShipmentRequest request)
        => CreateInternalAsync(request);

    [HasPermission(Permissions.Shipments.Update)]
    [CacheRemove(nameof(CacheKeys.ShipmentsPattern))]
    [ValidationAspect(typeof(Validators.FluentValidation.Shipments.UpdateShipmentRequestValidator))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<ShipmentResponse>> UpdateAsync(Guid id, UpdateShipmentRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("ShipmentNotFound", id);
        return await UpdateInternalAsync(entity, request);
    }

    [HasPermission(Permissions.Shipments.Delete)]
    [CacheRemove(nameof(CacheKeys.ShipmentsPattern))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("ShipmentNotFound", id);
        return await DeleteInternalAsync(id);
    }
}
