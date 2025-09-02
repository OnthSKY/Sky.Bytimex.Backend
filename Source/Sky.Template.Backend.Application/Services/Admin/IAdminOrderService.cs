using Sky.Template.Backend.Contract.Responses.SaleResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.Orders;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminOrderService
{
    [HasPermission(Permissions.Orders.View)]
    Task<BaseControllerResponse<OrderListResponse>> GetAllSalesAsync();
    [HasPermission(Permissions.Orders.View)]
    Task<BaseControllerResponse<OrderListPaginatedResponse>> GetFilteredPaginatedAsync(OrderFilterRequest request);
    [HasPermission(Permissions.Orders.View)]
    Task<BaseControllerResponse<SingleOrderResponse>> GetSaleByIdAsync(Guid id);
    [HasPermission(Permissions.Orders.Create)]
    Task<BaseControllerResponse<SingleOrderResponse>> CreateSaleAsync(CreateOrderRequest request);
    [HasPermission(Permissions.Orders.Update)]
    Task<BaseControllerResponse<SingleOrderResponse>> UpdateSaleAsync(Guid id, UpdateOrderRequest request);
    [HasPermission(Permissions.Orders.Delete)]
    Task<BaseControllerResponse> SoftDeleteSaleAsync(Guid id);
    [HasPermission(Permissions.Orders.Delete)]
    Task<BaseControllerResponse> HardDeleteSaleAsync(Guid id);
}

public class AdminOrderService : IAdminOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdminOrderService(IOrderRepository orderRepository, IHttpContextAccessor httpContextAccessor)
    {
        _orderRepository = orderRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    [HasPermission(Permissions.Orders.View)]
    public async Task<BaseControllerResponse<OrderListResponse>> GetAllSalesAsync()
    {
        var sales = await _orderRepository.GetAllAsync();
        var mapped = sales.Select(Map).ToList();
        return ControllerResponseBuilder.Success(new OrderListResponse
        {
            Orders = mapped,
            TotalCount = mapped.Count,
            TotalAmount = mapped.Sum(s => s.TotalAmount)
        });
    }

    [HasPermission(Permissions.Orders.View)]
    public async Task<BaseControllerResponse<OrderListPaginatedResponse>> GetFilteredPaginatedAsync(OrderFilterRequest request)
    {
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
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            throw new NotFoundException("SaleNotFound", id);
        return ControllerResponseBuilder.Success(Map(order));
    }

    [HasPermission(Permissions.Orders.Create)]
    public async Task<BaseControllerResponse<SingleOrderResponse>> CreateSaleAsync(CreateOrderRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var entity = new OrderEntity
        {
            VendorId = request.VendorId,
            BuyerId = request.BuyerId,
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

    [HasPermission(Permissions.Orders.Update)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<SingleOrderResponse>> UpdateSaleAsync(Guid id, UpdateOrderRequest request)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            throw new NotFoundException("SaleNotFound", id);
        var userId = _httpContextAccessor.HttpContext.GetUserId();
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
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            throw new NotFoundException("SaleNotFound", id);
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        order.IsDeleted = true;
        order.DeletedAt = DateTime.UtcNow;
        order.DeletedBy = userId;
        await _orderRepository.UpdateAsync(order);
        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.Orders.Delete)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> HardDeleteSaleAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            throw new NotFoundException("SaleNotFound", id);
        await _orderRepository.DeleteAsync(id);
        return ControllerResponseBuilder.Success();
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

