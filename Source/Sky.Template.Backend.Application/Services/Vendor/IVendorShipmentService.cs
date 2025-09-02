using Microsoft.AspNetCore.Http;
using System.Linq;
using Sky.Template.Backend.Contract.Requests.Shipments;
using Sky.Template.Backend.Contract.Responses.ShipmentResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.Vendor;

public interface IVendorShipmentService
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

public class VendorShipmentService : ShipmentServiceBase, IVendorShipmentService
{
    public VendorShipmentService(IShipmentRepository repository, IHttpContextAccessor httpContextAccessor)
        : base(repository, httpContextAccessor) { }

    private Guid GetVendorId() => _httpContextAccessor.HttpContext.GetUserId();

    [HasPermission(Permissions.Shipments.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.ShipmentsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<ShipmentListPaginatedResponse>> GetFilteredPaginatedAsync(ShipmentFilterRequest request)
    {
        var vendorId = GetVendorId();
        var (entities, totalCount) = await _repository.GetFilteredPaginatedAsync(request);
        var filtered = entities.Where(s => s.CreatedBy == vendorId).Select(Map).ToList();
        var paginated = new PaginatedData<ShipmentResponse>
        {
            Items = filtered,
            TotalCount = filtered.Count,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPage = (int)Math.Ceiling((double)filtered.Count / request.PageSize)
        };
        return ControllerResponseBuilder.Success(new ShipmentListPaginatedResponse { Shipments = paginated });
    }

    [HasPermission(Permissions.Shipments.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.ShipmentsPrefix), ExpirationInMinutes = 60)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<ShipmentResponse>> GetByIdAsync(Guid id)
    {
        var vendorId = GetVendorId();
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null || entity.CreatedBy != vendorId)
            throw new NotFoundException("ShipmentNotFound", id);
        return ControllerResponseBuilder.Success(Map(entity));
    }

    [HasPermission(Permissions.Shipments.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.ShipmentsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<IEnumerable<ShipmentResponse>>> GetByOrderIdAsync(Guid orderId)
    {
        var vendorId = GetVendorId();
        var entities = await _repository.GetByOrderIdAsync(orderId);
        var list = entities.Where(s => s.CreatedBy == vendorId).Select(Map);
        return ControllerResponseBuilder.Success(list);
    }

    [HasPermission(Permissions.Shipments.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.ShipmentsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<IEnumerable<ShipmentResponse>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var vendorId = GetVendorId();
        var entities = await _repository.GetByDateRangeAsync(startDate, endDate);
        var list = entities.Where(s => s.CreatedBy == vendorId).Select(Map);
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
        var vendorId = GetVendorId();
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null || entity.CreatedBy != vendorId)
            throw new NotFoundException("ShipmentNotFound", id);
        return await UpdateInternalAsync(entity, request);
    }

    [HasPermission(Permissions.Shipments.Delete)]
    [CacheRemove(nameof(CacheKeys.ShipmentsPattern))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> DeleteAsync(Guid id)
    {
        var vendorId = GetVendorId();
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null || entity.CreatedBy != vendorId)
            throw new NotFoundException("ShipmentNotFound", id);
        return await DeleteInternalAsync(id);
    }
}
