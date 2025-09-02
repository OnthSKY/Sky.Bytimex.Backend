using System.Net;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.Invoices;
using Sky.Template.Backend.Contract.Responses.InvoiceResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.Vendor;

public interface IVendorInvoiceService
{
    [HasPermission(Permissions.Invoices.View)]
    Task<BaseControllerResponse<InvoiceListPaginatedResponse>> GetFilteredPaginatedAsync(InvoiceFilterRequest request);
    [HasPermission(Permissions.Invoices.View)]
    Task<BaseControllerResponse<InvoiceResponse>> GetByIdAsync(Guid id);
    [HasPermission(Permissions.Invoices.Create)]
    Task<BaseControllerResponse<InvoiceResponse>> CreateAsync(CreateInvoiceRequest request);
    [HasPermission(Permissions.Invoices.Update)]
    Task<BaseControllerResponse<InvoiceResponse>> UpdateAsync(Guid id, UpdateInvoiceRequest request);
    [HasPermission(Permissions.Invoices.Delete)]
    Task<BaseControllerResponse> DeleteAsync(Guid id);
}

public class VendorInvoiceService : IVendorInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public VendorInvoiceService(IInvoiceRepository invoiceRepository, IOrderRepository orderRepository, IHttpContextAccessor httpContextAccessor)
    {
        _invoiceRepository = invoiceRepository;
        _orderRepository = orderRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    [HasPermission(Permissions.Invoices.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.InvoicesPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<InvoiceListPaginatedResponse>> GetFilteredPaginatedAsync(InvoiceFilterRequest request)
    {
        var (entities, totalCount) = await _invoiceRepository.GetFilteredPaginatedAsync(request);
        var list = entities.Select(Map).ToList();
        var paginated = new PaginatedData<InvoiceResponse>
        {
            Items = list,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
        return ControllerResponseBuilder.Success(new InvoiceListPaginatedResponse { Invoices = paginated });
    }

    [HasPermission(Permissions.Invoices.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.InvoicesPrefix), ExpirationInMinutes = 60)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<InvoiceResponse>> GetByIdAsync(Guid id)
    {
        var entity = await _invoiceRepository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("InvoiceNotFound", id);
        return ControllerResponseBuilder.Success(Map(entity));
    }

    [HasPermission(Permissions.Invoices.Create)]
    [CacheRemove(nameof(CacheKeys.InvoicesPattern))]
    [ValidationAspect(typeof(Validators.FluentValidation.Invoices.CreateInvoiceRequestValidator))]
    public async Task<BaseControllerResponse<InvoiceResponse>> CreateAsync(CreateInvoiceRequest request)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null)
            throw new NotFoundException("OrderNotFound", request.OrderId);
        var existing = await _invoiceRepository.GetByOrderIdAsync(request.OrderId);
        if (existing != null)
            throw new BusinessRulesException("InvoiceAlreadyExists");
        if (!string.IsNullOrEmpty(request.InvoiceNumber) && !await _invoiceRepository.IsInvoiceNumberUniqueAsync(request.InvoiceNumber))
            throw new BusinessRulesException("InvoiceNumberMustBeUnique");
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var entity = new InvoiceEntity
        {
            OrderId = request.OrderId,
            InvoiceNumber = request.InvoiceNumber,
            InvoiceDate = request.InvoiceDate,
            DueDate = request.DueDate,
            BuyerId = request.BuyerId,
            TotalAmount = request.TotalAmount,
            Currency = request.Currency,
            PdfUrl = request.PdfUrl,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };
        var created = await _invoiceRepository.CreateAsync(entity);
        return ControllerResponseBuilder.Success(Map(created), "Created", HttpStatusCode.Created);
    }

    [HasPermission(Permissions.Invoices.Update)]
    [CacheRemove(nameof(CacheKeys.InvoicesPattern))]
    [ValidationAspect(typeof(Validators.FluentValidation.Invoices.UpdateInvoiceRequestValidator))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<InvoiceResponse>> UpdateAsync(Guid id, UpdateInvoiceRequest request)
    {
        var entity = await _invoiceRepository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("InvoiceNotFound", id);
        if (!string.IsNullOrEmpty(request.InvoiceNumber) && request.InvoiceNumber != entity.InvoiceNumber && !await _invoiceRepository.IsInvoiceNumberUniqueAsync(request.InvoiceNumber, id))
            throw new BusinessRulesException("InvoiceNumberMustBeUnique");
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        entity.InvoiceNumber = request.InvoiceNumber;
        entity.InvoiceDate = request.InvoiceDate;
        entity.DueDate = request.DueDate;
        entity.BuyerId = request.BuyerId;
        entity.TotalAmount = request.TotalAmount;
        entity.Currency = request.Currency;
        entity.PdfUrl = request.PdfUrl;
        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;
        var updated = await _invoiceRepository.UpdateAsync(entity);
        return ControllerResponseBuilder.Success(Map(updated));
    }

    [HasPermission(Permissions.Invoices.Delete)]
    [CacheRemove(nameof(CacheKeys.InvoicesPattern))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> DeleteAsync(Guid id)
    {
        var entity = await _invoiceRepository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("InvoiceNotFound", id);
        await _invoiceRepository.SoftDeleteAsync(id);
        return ControllerResponseBuilder.Success();
    }

    private static InvoiceResponse Map(InvoiceEntity entity) => new()
    {
        Id = entity.Id,
        OrderId = entity.OrderId,
        InvoiceNumber = entity.InvoiceNumber,
        InvoiceDate = entity.InvoiceDate,
        DueDate = entity.DueDate,
        BuyerId = entity.BuyerId,
        TotalAmount = entity.TotalAmount,
        Currency = entity.Currency,
        PdfUrl = entity.PdfUrl,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy
    };
}

