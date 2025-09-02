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

namespace Sky.Template.Backend.Application.Services.Vendor;

public interface IVendorOrderService
{
    [HasPermission(Permissions.Orders.View)]
    Task<BaseControllerResponse<OrderListResponse>> GetAllSalesAsync();
    [HasPermission(Permissions.Orders.View)]
    Task<BaseControllerResponse<OrderListPaginatedResponse>> GetFilteredPaginatedAsync(OrderFilterRequest request);
    [HasPermission(Permissions.Orders.View)]
    Task<BaseControllerResponse<SingleOrderResponse>> GetSaleByIdAsync(Guid id);
    [HasPermission(Permissions.Orders.Update)]
    Task<BaseControllerResponse<SingleOrderResponse>> UpdateSaleAsync(Guid id, UpdateOrderRequest request);
    [HasPermission(Permissions.Orders.Delete)]
    Task<BaseControllerResponse> SoftDeleteSaleAsync(Guid id);
    [HasPermission(Permissions.Orders.Cancel)]
    Task<BaseControllerResponse> CancelOrderAsync(Guid orderId);
    [HasPermission(Permissions.Orders.Reorder)]
    Task<BaseControllerResponse<Guid>> ReorderAsync(Guid orderId);
}

public class VendorOrderService : IVendorOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public VendorOrderService(IOrderRepository orderRepository, IOrderDetailRepository orderDetailRepository, IHttpContextAccessor httpContextAccessor)
    {
        _orderRepository = orderRepository;
        _orderDetailRepository = orderDetailRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid GetVendorId() => _httpContextAccessor.HttpContext.GetUserId();

    [HasPermission(Permissions.Orders.View)]
    public async Task<BaseControllerResponse<OrderListResponse>> GetAllSalesAsync()
    {
        var vendorId = GetVendorId();
        var sales = await _orderRepository.GetAllAsync();
        var filtered = sales.Where(s => s.VendorId == vendorId).Select(Map).ToList();
        return ControllerResponseBuilder.Success(new OrderListResponse
        {
            Orders = filtered,
            TotalCount = filtered.Count,
            TotalAmount = filtered.Sum(s => s.TotalAmount)
        });
    }

    [HasPermission(Permissions.Orders.View)]
    public async Task<BaseControllerResponse<OrderListPaginatedResponse>> GetFilteredPaginatedAsync(OrderFilterRequest request)
    {
        var vendorId = GetVendorId();
        request.Filters["vendorId"] = vendorId.ToString();
        var (data, totalCount) = await _orderRepository.GetFilteredPaginatedAsync(request);
        var mapped = data.Select(Map).ToList();
        var paginated = new PaginatedData<SingleOrderResponse>
        {
            Items = mapped,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
        return ControllerResponseBuilder.Success(new OrderListPaginatedResponse { Orders = paginated });
    }

    [HasPermission(Permissions.Orders.View)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<SingleOrderResponse>> GetSaleByIdAsync(Guid id)
    {
        var vendorId = GetVendorId();
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null || order.VendorId != vendorId)
            throw new NotFoundException("SaleNotFound", id);
        return ControllerResponseBuilder.Success(Map(order));
    }

    [HasPermission(Permissions.Orders.Update)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<SingleOrderResponse>> UpdateSaleAsync(Guid id, UpdateOrderRequest request)
    {
        var vendorId = GetVendorId();
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null || order.VendorId != vendorId)
            throw new NotFoundException("SaleNotFound", id);
        var userId = vendorId;
        order.VendorId = request.VendorId;
        order.BuyerId = request.BuyerId;
        order.BuyerDescription = request.BuyerDescription;
        order.TotalAmount = request.TotalAmount;
        order.Currency = request.Currency;
        order.OrderStatus = request.OrderStatus;
        order.OrderDate = request.OrderDate;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = userId;
        await _orderRepository.UpdateAsync(order);
        return ControllerResponseBuilder.Success(Map(order), "SaleUpdatedSuccessfully");
    }

    [HasPermission(Permissions.Orders.Delete)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> SoftDeleteSaleAsync(Guid id)
    {
        var vendorId = GetVendorId();
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null || order.VendorId != vendorId)
            throw new NotFoundException("SaleNotFound", id);
        var userId = vendorId;
        order.IsDeleted = true;
        order.DeletedAt = DateTime.UtcNow;
        order.DeletedBy = userId;
        await _orderRepository.UpdateAsync(order);
        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.Orders.Cancel)]
    [EnsureUserIsValid(new[] { "orderId" })]
    public async Task<BaseControllerResponse> CancelOrderAsync(Guid orderId)
    {
        var vendorId = GetVendorId();
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null || order.VendorId != vendorId)
            throw new NotFoundException("SaleNotFound", orderId);
        if (order.OrderStatus == OrderStatus.SHIPPED.ToString() || order.OrderStatus == OrderStatus.DELIVERED.ToString())
            throw new BusinessRulesException("OrderCannotBeCanceled");
        order.OrderStatus = OrderStatus.CANCELLED.ToString();
        order.UpdatedBy = vendorId;
        await _orderRepository.UpdateAsync(order);
        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.Orders.Reorder)]
    [EnsureUserIsValid(new[] { "orderId" })]
    public async Task<BaseControllerResponse<Guid>> ReorderAsync(Guid orderId)
    {
        var vendorId = GetVendorId();
        var original = await _orderRepository.GetByIdAsync(orderId);
        if (original == null || original.VendorId != vendorId)
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
            CreatedBy = vendorId,
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
                CreatedBy = vendorId,
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

