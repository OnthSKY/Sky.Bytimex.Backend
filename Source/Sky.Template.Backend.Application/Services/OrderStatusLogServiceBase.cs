using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Responses.OrderStatusLogResponses;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services;

public abstract class OrderStatusLogServiceBase
{
    protected readonly IOrderStatusLogRepository _logRepository;
    protected readonly IOrderRepository _orderRepository;
    protected readonly IHttpContextAccessor _httpContextAccessor;

    protected OrderStatusLogServiceBase(IOrderStatusLogRepository logRepository, IOrderRepository orderRepository, IHttpContextAccessor httpContextAccessor)
    {
        _logRepository = logRepository;
        _orderRepository = orderRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    protected Guid GetUserId() => _httpContextAccessor.HttpContext.GetUserId();

    protected async Task<OrderEntity> GetOrderOrThrowAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new NotFoundException("OrderNotFound", orderId);
        return order;
    }

    protected static OrderStatusLogResponse MapToResponse(OrderStatusLogEntity entity) => new()
    {
        Id = entity.Id,
        OrderId = entity.OrderId,
        OldStatus = entity.OldStatus,
        NewStatus = entity.NewStatus,
        ChangedBy = entity.ChangedBy,
        ChangedAt = entity.ChangedAt,
        Note = entity.Note,
        CreatedAt = entity.ChangedAt,
    };
}

