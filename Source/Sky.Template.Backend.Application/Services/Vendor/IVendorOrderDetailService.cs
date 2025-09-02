using Sky.Template.Backend.Core.Constants;
using System.Net;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.OrderDetails;
using Sky.Template.Backend.Contract.Responses.OrderDetailResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.Vendor;

public interface IVendorOrderDetailService
{
    [HasPermission(Permissions.OrderDetails.View)]
    Task<BaseControllerResponse<OrderDetailListResponse>> GetByOrderIdAsync(Guid orderId);

    [HasPermission(Permissions.OrderDetails.View)]
    [EnsureUserIsValid(new[] { "id" })]
    Task<BaseControllerResponse<OrderDetailResponse>> GetByIdAsync(Guid id);

    [HasPermission(Permissions.OrderDetails.Create)]
    [ValidationAspect(typeof(Validators.FluentValidation.OrderDetails.CreateOrderDetailRequestValidator))]
    Task<BaseControllerResponse<OrderDetailResponse>> CreateAsync(CreateOrderDetailRequest request);

    [HasPermission(Permissions.OrderDetails.Update)]
    [ValidationAspect(typeof(Validators.FluentValidation.OrderDetails.UpdateOrderDetailRequestValidator))]
    [EnsureUserIsValid(new[] { "id" })]
    Task<BaseControllerResponse<OrderDetailResponse>> UpdateAsync(Guid id, UpdateOrderDetailRequest request);

    [HasPermission(Permissions.OrderDetails.Delete)]
    [EnsureUserIsValid(new[] { "id" })]
    Task<BaseControllerResponse> DeleteAsync(Guid id);
}

public class VendorOrderDetailService : IVendorOrderDetailService
{
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public VendorOrderDetailService(IOrderDetailRepository orderDetailRepository, IProductRepository productRepository, IOrderRepository orderRepository, IHttpContextAccessor httpContextAccessor)
    {
        _orderDetailRepository = orderDetailRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid GetVendorId() => _httpContextAccessor.HttpContext.GetUserId();

    public async Task<BaseControllerResponse<OrderDetailListResponse>> GetByOrderIdAsync(Guid orderId)
    {
        var vendorId = GetVendorId();
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null || order.VendorId != vendorId)
            throw new NotFoundException("OrderNotFound", orderId);

        var details = await _orderDetailRepository.GetByOrderIdAsync(orderId);
        var response = new OrderDetailListResponse
        {
            OrderDetails = details.Select(MapToResponse).ToList()
        };
        return ControllerResponseBuilder.Success(response);
    }

    public async Task<BaseControllerResponse<OrderDetailResponse>> GetByIdAsync(Guid id)
    {
        var vendorId = GetVendorId();
        var detail = await _orderDetailRepository.GetByIdAsync(id);
        if (detail == null)
            throw new NotFoundException("OrderDetailNotFound", id);

        var order = await _orderRepository.GetByIdAsync(detail.OrderId);
        if (order == null || order.VendorId != vendorId)
            throw new NotFoundException("OrderDetailNotFound", id);

        return ControllerResponseBuilder.Success(MapToResponse(detail));
    }

    public async Task<BaseControllerResponse<OrderDetailResponse>> CreateAsync(CreateOrderDetailRequest request)
    {
        var vendorId = GetVendorId();
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null || order.VendorId != vendorId)
            throw new NotFoundException("OrderNotFound", request.OrderId);

        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product == null || product.IsDeleted || product.Status != "ACTIVE")
            throw new NotFoundException("ProductNotFound", request.ProductId);

        var entity = new OrderDetailEntity
        {
            OrderId = request.OrderId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            TotalPrice = request.Quantity * request.UnitPrice,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = vendorId,
            IsDeleted = false
        };

        var created = await _orderDetailRepository.CreateAsync(entity);
        var response = MapToResponse(created);
        return ControllerResponseBuilder.Success(response, "OrderDetailCreatedSuccessfully", HttpStatusCode.Created);
    }

    public async Task<BaseControllerResponse<OrderDetailResponse>> UpdateAsync(Guid id, UpdateOrderDetailRequest request)
    {
        var vendorId = GetVendorId();
        var entity = await _orderDetailRepository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("OrderDetailNotFound", id);

        var order = await _orderRepository.GetByIdAsync(entity.OrderId);
        if (order == null || order.VendorId != vendorId)
            throw new NotFoundException("OrderDetailNotFound", id);

        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product == null || product.IsDeleted || product.Status != "ACTIVE")
            throw new NotFoundException("ProductNotFound", request.ProductId);

        entity.ProductId = request.ProductId;
        entity.Quantity = request.Quantity;
        entity.UnitPrice = request.UnitPrice;
        entity.TotalPrice = request.Quantity * request.UnitPrice;
        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = vendorId;

        var updated = await _orderDetailRepository.UpdateAsync(entity);
        return ControllerResponseBuilder.Success(MapToResponse(updated), "OrderDetailUpdatedSuccessfully");
    }

    public async Task<BaseControllerResponse> DeleteAsync(Guid id)
    {
        var vendorId = GetVendorId();
        var entity = await _orderDetailRepository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("OrderDetailNotFound", id);

        var order = await _orderRepository.GetByIdAsync(entity.OrderId);
        if (order == null || order.VendorId != vendorId)
            throw new NotFoundException("OrderDetailNotFound", id);

        await _orderDetailRepository.DeleteAsync(id);
        return ControllerResponseBuilder.Success();
    }

    private static OrderDetailResponse MapToResponse(OrderDetailEntity entity)
        => new()
        {
            Id = entity.Id,
            OrderId = entity.OrderId,
            ProductId = entity.ProductId,
            Quantity = entity.Quantity,
            UnitPrice = entity.UnitPrice,
            TotalPrice = entity.TotalPrice,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            IsDeleted = entity.IsDeleted,
            DeletedAt = entity.DeletedAt,
            DeletedBy = entity.DeletedBy,
            DeleteReason = entity.DeleteReason
        };
}

