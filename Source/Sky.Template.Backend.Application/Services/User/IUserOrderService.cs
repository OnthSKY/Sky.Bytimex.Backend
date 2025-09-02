using Sky.Template.Backend.Contract.Responses.SaleResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.Enums;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.Orders;

namespace Sky.Template.Backend.Application.Services.User;

public interface IUserOrderService
{
    [HasPermission(Permissions.Orders.View)]
    Task<BaseControllerResponse<SingleOrderResponse>> GetSaleByIdAsync(Guid id);
    [HasPermission(Permissions.Orders.Create)]
    Task<BaseControllerResponse<SingleOrderResponse>> CreateSaleAsync(CreateOrderRequest request);
    [HasPermission(Permissions.Orders.Cancel)]
    Task<BaseControllerResponse> CancelOrderAsync(Guid orderId);
    [HasPermission(Permissions.Orders.Reorder)]
    Task<BaseControllerResponse<Guid>> ReorderAsync(Guid orderId);
}

public class UserOrderService : IUserOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserOrderService(IOrderRepository orderRepository, IOrderDetailRepository orderDetailRepository, IHttpContextAccessor httpContextAccessor)
    {
        _orderRepository = orderRepository;
        _orderDetailRepository = orderDetailRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid GetUserId() => _httpContextAccessor.HttpContext.GetUserId();

    [HasPermission(Permissions.Orders.View)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<SingleOrderResponse>> GetSaleByIdAsync(Guid id)
    {
        var userId = GetUserId();
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null || order.BuyerId != userId)
            throw new NotFoundException("SaleNotFound", id);
        return ControllerResponseBuilder.Success(Map(order));
    }

    [HasPermission(Permissions.Orders.Create)]
    public async Task<BaseControllerResponse<SingleOrderResponse>> CreateSaleAsync(CreateOrderRequest request)
    {
        var userId = GetUserId();
        var entity = new OrderEntity
        {
            VendorId = request.VendorId,
            BuyerId = userId,
            BuyerDescription = request.BuyerDescription,
            TotalAmount = request.TotalAmount,
            Currency = request.Currency,
            OrderStatus = request.SaleStatus,
            OrderDate = request.OrderDate,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };
        await _orderRepository.CreateAsync(entity);
        return ControllerResponseBuilder.Success(Map(entity), "SaleCreatedSuccessfully");
    }

    [HasPermission(Permissions.Orders.Cancel)]
    [EnsureUserIsValid(new[] { "orderId" })]
    public async Task<BaseControllerResponse> CancelOrderAsync(Guid orderId)
    {
        var userId = GetUserId();
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null || order.BuyerId != userId)
            throw new NotFoundException("SaleNotFound", orderId);
        if (order.OrderStatus == OrderStatus.SHIPPED.ToString() || order.OrderStatus == OrderStatus.DELIVERED.ToString())
            throw new BusinessRulesException("OrderCannotBeCanceled");
        order.OrderStatus = OrderStatus.CANCELLED.ToString();
        order.UpdatedBy = userId;
        await _orderRepository.UpdateAsync(order);
        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.Orders.Reorder)]
    [EnsureUserIsValid(new[] { "orderId" })]
    public async Task<BaseControllerResponse<Guid>> ReorderAsync(Guid orderId)
    {
        var userId = GetUserId();
        var original = await _orderRepository.GetByIdAsync(orderId);
        if (original == null || original.BuyerId != userId)
            throw new NotFoundException("SaleNotFound", orderId);

        var details = await _orderDetailRepository.GetByOrderIdAsync(orderId);

        var newOrder = new OrderEntity
        {
            VendorId = original.VendorId,
            BuyerId = original.BuyerId,
            BuyerDescription = original.BuyerDescription,
            TotalAmount = original.TotalAmount,
            Currency = original.Currency,
            OrderStatus = OrderStatus.PENDING.ToString(),
            OrderDate = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };

        var created = await _orderRepository.CreateAsync(newOrder);

        foreach (var detail in details)
        {
            var newDetail = new OrderDetailEntity
            {
                OrderId = created.Id,
                ProductId = detail.ProductId,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice,
                TotalPrice = detail.TotalPrice,
                Status = detail.Status,
                CreatedBy = userId,
                IsDeleted = false
            };
            await _orderDetailRepository.CreateAsync(newDetail);
        }

        return ControllerResponseBuilder.Success(created.Id, "SaleReorderedSuccessfully");
    }

    private static SingleOrderResponse Map(OrderEntity order) => new()
    {
        Id = order.Id,
        VendorId = order.VendorId,
        BuyerId = order.BuyerId,
        BuyerDescription = order.BuyerDescription,
        TotalAmount = order.TotalAmount,
        Currency = order.Currency,
        OrderStatus = order.OrderStatus,
        DiscountCode = order.DiscountCode,
        DiscountAmount = order.DiscountAmount,
        PaymentStatus = order.PaymentStatus,
        OrderDate = order.OrderDate,
        IsDeleted = order.IsDeleted,
        CreatedAt = order.CreatedAt,
        CreatedBy = order.CreatedBy,
        UpdatedAt = order.UpdatedAt,
        UpdatedBy = order.UpdatedBy,
        DeletedAt = order.DeletedAt,
        DeletedBy = order.DeletedBy,
        DeleteReason = order.DeleteReason
    };
}

