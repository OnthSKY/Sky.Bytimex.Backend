using Sky.Template.Backend.Core.Constants;
using System;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.OrderStatusLogs;
using Sky.Template.Backend.Contract.Responses.OrderStatusLogResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.Vendor;

public interface IVendorOrderStatusLogService
{
    Task<BaseControllerResponse<OrderStatusLogListResponse>> GetByOrderIdAsync(Guid orderId);
    Task<BaseControllerResponse<OrderStatusLogResponse>> CreateAsync(CreateOrderStatusLogRequest request);
}

public class VendorOrderStatusLogService : OrderStatusLogServiceBase, IVendorOrderStatusLogService
{
    public VendorOrderStatusLogService(IOrderStatusLogRepository logRepository, IOrderRepository orderRepository, IHttpContextAccessor httpContextAccessor)
        : base(logRepository, orderRepository, httpContextAccessor) { }

    private Guid GetVendorId() => GetUserId();

    [HasPermission(Permissions.OrderStatusLogs.View)]
    public async Task<BaseControllerResponse<OrderStatusLogListResponse>> GetByOrderIdAsync(Guid orderId)
    {
        var vendorId = GetVendorId();
        var order = await GetOrderOrThrowAsync(orderId);
        if (order.VendorId != vendorId)
            throw new NotFoundException("OrderNotFound", orderId);
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
        var vendorId = GetVendorId();
        var order = await GetOrderOrThrowAsync(request.OrderId);
        if (order.VendorId != vendorId)
            throw new NotFoundException("OrderNotFound", request.OrderId);
        if (request.OldStatus == request.NewStatus)
            throw new ValidationException("StatusesMustDiffer");
        var entity = new OrderStatusLogEntity
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            OldStatus = request.OldStatus,
            NewStatus = request.NewStatus,
            ChangedBy = vendorId,
            ChangedAt = DateTime.UtcNow,
            Note = request.Note
        };
        var created = await _logRepository.CreateAsync(entity);
        return ControllerResponseBuilder.Success(MapToResponse(created), "OrderStatusLogCreated", HttpStatusCode.Created);
    }
}

