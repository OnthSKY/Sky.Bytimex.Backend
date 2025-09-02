using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Responses.ShipmentResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Repositories;
using System.Linq;
using System.Collections.Generic;

namespace Sky.Template.Backend.Application.Services.User;

public interface IUserShipmentService
{
    [HasPermission(Permissions.Shipments.View)]
    Task<BaseControllerResponse<IEnumerable<ShipmentResponse>>> GetShipmentByOrderIdAsync(Guid orderId);
    [HasPermission(Permissions.Shipments.View)]
    Task<BaseControllerResponse<ShipmentResponse>> TrackShipmentAsync(string trackingNumber);
}

public class UserShipmentService : ShipmentServiceBase, IUserShipmentService
{
    public UserShipmentService(IShipmentRepository repository, IHttpContextAccessor httpContextAccessor)
        : base(repository, httpContextAccessor) { }

    [HasPermission(Permissions.Shipments.View)]
    [EnsureUserIsValid(new[] { "orderId" })]
    public async Task<BaseControllerResponse<IEnumerable<ShipmentResponse>>> GetShipmentByOrderIdAsync(Guid orderId)
    {
        var buyerId = GetUserId();
        var entities = await _repository.GetByOrderIdAndBuyerIdAsync(orderId, buyerId);
        if (!entities.Any())
            throw new NotFoundException("ShipmentNotFound", orderId);
        var list = entities.Select(Map);
        return ControllerResponseBuilder.Success(list);
    }

    [HasPermission(Permissions.Shipments.View)]
    [EnsureUserIsValid(new[] { "trackingNumber" })]
    public async Task<BaseControllerResponse<ShipmentResponse>> TrackShipmentAsync(string trackingNumber)
    {
        var buyerId = GetUserId();
        var entity = await _repository.GetByTrackingNumberAndBuyerIdAsync(trackingNumber, buyerId);
        if (entity == null)
            throw new NotFoundException("ShipmentNotFound", trackingNumber);
        return ControllerResponseBuilder.Success(Map(entity));
    }
}
