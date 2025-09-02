using Sky.Template.Backend.Core.Constants;
using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.OrderStatusLogs;
using Sky.Template.Backend.Contract.Responses.OrderStatusLogResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminOrderStatusLogService
{
    Task<BaseControllerResponse<OrderStatusLogListResponse>> GetByOrderIdAsync(Guid orderId);
    Task<BaseControllerResponse<OrderStatusLogResponse>> CreateAsync(CreateOrderStatusLogRequest request);
}

public class AdminOrderStatusLogService : OrderStatusLogServiceBase, IAdminOrderStatusLogService
{
    public AdminOrderStatusLogService(IOrderStatusLogRepository logRepository, IOrderRepository orderRepository, IHttpContextAccessor httpContextAccessor)
        : base(logRepository, orderRepository, httpContextAccessor) { }

    [HasPermission(Permissions.OrderStatusLogs.View)]
    public async Task<BaseControllerResponse<OrderStatusLogListResponse>> GetByOrderIdAsync(Guid orderId)
    {
        await GetOrderOrThrowAsync(orderId);
        var logs = await _logRepository.GetByOrderIdAsync(orderId);
        var response = new OrderStatusLogListResponse
        {
            Logs = logs.Select(MapToResponse).ToList()
        };
        return ControllerResponseBuilder.Success(response);
    }

    [HasPermission(Permissions.OrderStatusLogs.Create)]
    [ValidationAspect(typeof(Validators.FluentValidation.OrderStatusLogs.CreateOrderStatusLogRequestValidator))]
    public async Task<BaseControllerResponse<OrderStatusLogResponse>> CreateAsync(CreateOrderStatusLogRequest request)
    {
        await GetOrderOrThrowAsync(request.OrderId);
        if (request.OldStatus == request.NewStatus)
            throw new ValidationException("StatusesMustDiffer");
        var userId = GetUserId();
        var entity = new OrderStatusLogEntity
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            OldStatus = request.OldStatus,
            NewStatus = request.NewStatus,
            ChangedBy = userId,
            ChangedAt = DateTime.UtcNow,
            Note = request.Note
        };
        var created = await _logRepository.CreateAsync(entity);
        return ControllerResponseBuilder.Success(MapToResponse(created), "OrderStatusLogCreated", HttpStatusCode.Created);
    }
}

