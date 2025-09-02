using System.Net;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.Shipments;
using Sky.Template.Backend.Contract.Responses.ShipmentResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services;

public abstract class ShipmentServiceBase
{
    protected readonly IShipmentRepository _repository;
    protected readonly IHttpContextAccessor _httpContextAccessor;

    protected ShipmentServiceBase(IShipmentRepository repository, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    protected Guid GetUserId() => _httpContextAccessor.HttpContext.GetUserId();

    protected async Task<BaseControllerResponse<ShipmentResponse>> CreateInternalAsync(CreateShipmentRequest request)
    {
        var existing = await _repository.GetByOrderIdAsync(request.OrderId);
        if (existing.Any())
            throw new BusinessRulesException("ShipmentAlreadyExists");
        var userId = GetUserId();
        var entity = new ShipmentEntity
        {
            OrderId = request.OrderId,
            ShipmentDate = request.ShipmentDate,
            Carrier = request.Carrier,
            TrackingNumber = request.TrackingNumber,
            Status = request.Status,
            EstimatedDeliveryDate = request.EstimatedDeliveryDate,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };
        var created = await _repository.CreateAsync(entity);
        return ControllerResponseBuilder.Success(Map(entity: created), "Created", HttpStatusCode.Created);
    }

    protected async Task<BaseControllerResponse<ShipmentResponse>> UpdateInternalAsync(ShipmentEntity entity, UpdateShipmentRequest request)
    {
        var userId = GetUserId();
        if (request.ShipmentDate.HasValue)
            entity.ShipmentDate = request.ShipmentDate.Value;
        if (!string.IsNullOrEmpty(request.Carrier))
            entity.Carrier = request.Carrier;
        if (!string.IsNullOrEmpty(request.TrackingNumber))
            entity.TrackingNumber = request.TrackingNumber;
        if (!string.IsNullOrEmpty(request.Status))
            entity.Status = request.Status;
        if (request.EstimatedDeliveryDate.HasValue)
            entity.EstimatedDeliveryDate = request.EstimatedDeliveryDate.Value;
        if (request.Notes != null)
            entity.Notes = request.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;
        var updated = await _repository.UpdateAsync(entity);
        return ControllerResponseBuilder.Success(Map(updated));
    }

    protected async Task<BaseControllerResponse> DeleteInternalAsync(Guid id)
    {
        await _repository.SoftDeleteAsync(id);
        return ControllerResponseBuilder.Success();
    }

    protected static ShipmentResponse Map(ShipmentEntity entity) => new()
    {
        Id = entity.Id,
        OrderId = entity.OrderId,
        ShipmentDate = entity.ShipmentDate,
        Carrier = entity.Carrier,
        TrackingNumber = entity.TrackingNumber,
        Status = entity.Status,
        EstimatedDeliveryDate = entity.EstimatedDeliveryDate,
        Notes = entity.Notes,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy
    };
}
