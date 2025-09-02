using System.Net;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.Payments;
using Sky.Template.Backend.Contract.Responses.PaymentResponses;
using System.Linq;
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
using Sky.Template.Backend.Application.Services.Payments;
using Sky.Template.Backend.Core.Enums;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminPaymentService
{
    [HasPermission(Permissions.Payments.View)]
    Task<BaseControllerResponse<PaymentListPaginatedResponse>> GetFilteredPaginatedAsync(PaymentFilterRequest request);
    [HasPermission(Permissions.Payments.View)]
    Task<BaseControllerResponse<PaymentResponse>> GetByIdAsync(Guid id);
    [HasPermission(Permissions.Payments.View)]
    Task<BaseControllerResponse<IEnumerable<PaymentResponse>>> GetByOrderIdAsync(Guid orderId);
    [HasPermission(Permissions.Payments.View)]
    Task<BaseControllerResponse<IEnumerable<PaymentResponse>>> GetByBuyerIdAsync(Guid buyerId);
    [HasPermission(Permissions.Payments.Create)]
    Task<BaseControllerResponse<PaymentResponse>> CreateAsync(CreatePaymentRequest request);
    [HasPermission(Permissions.Payments.Update)]
    Task<BaseControllerResponse<PaymentResponse>> UpdateAsync(Guid id, UpdatePaymentRequest request);
    [HasPermission(Permissions.Payments.Update)]
    Task ConfirmPaymentAsync(Guid paymentId, string transactionHash);
    [HasPermission(Permissions.Payments.Delete)]
    Task<BaseControllerResponse> DeleteAsync(Guid id);
}

public class AdminPaymentService : IAdminPaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPaymentGatewayResolver _gatewayResolver;

    public AdminPaymentService(IPaymentRepository repository, IHttpContextAccessor httpContextAccessor, IPaymentGatewayResolver gatewayResolver)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
        _gatewayResolver = gatewayResolver;
    }

    [HasPermission(Permissions.Payments.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.PaymentsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<PaymentListPaginatedResponse>> GetFilteredPaginatedAsync(PaymentFilterRequest request)
    {
        var (entities, totalCount) = await _repository.GetFilteredPaginatedAsync(request);
        var list = entities.Select(Map).ToList();
        var paginated = new PaginatedData<PaymentResponse>
        {
            Items = list,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
        return ControllerResponseBuilder.Success(new PaymentListPaginatedResponse { Payments = paginated });
    }

    [HasPermission(Permissions.Payments.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.PaymentsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<PaymentResponse>> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("PaymentNotFound", id);
        return ControllerResponseBuilder.Success(Map(entity));
    }

    [HasPermission(Permissions.Payments.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.PaymentsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<IEnumerable<PaymentResponse>>> GetByOrderIdAsync(Guid orderId)
    {
        var entities = await _repository.GetByOrderIdAsync(orderId);
        var list = entities.Select(Map);
        return ControllerResponseBuilder.Success(list);
    }

    [HasPermission(Permissions.Payments.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.PaymentsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<IEnumerable<PaymentResponse>>> GetByBuyerIdAsync(Guid buyerId)
    {
        var entities = await _repository.GetByBuyerIdAsync(buyerId);
        var list = entities.Select(Map);
        return ControllerResponseBuilder.Success(list);
    }

    [HasPermission(Permissions.Payments.Create)]
    [CacheRemove(nameof(CacheKeys.PaymentsPattern))]
    [ValidationAspect(typeof(Validators.FluentValidation.Payments.CreatePaymentRequestValidator))]
    public async Task<BaseControllerResponse<PaymentResponse>> CreateAsync(CreatePaymentRequest request)
    {
        var existing = await _repository.GetByOrderIdAsync(request.OrderId);
        if (existing.Any(p => p.PaymentStatus == Core.Enums.PaymentStatus.PAID.ToString()))
            throw new BusinessRulesException("PaymentAlreadyExists");
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var entity = new PaymentEntity
        {
            OrderId = request.OrderId,
            BuyerId = request.BuyerId,
            Amount = request.Amount,
            Currency = request.Currency,
            PaymentType = request.PaymentType,
            PaymentStatus = request.PaymentStatus,
            TxHash = request.TxHash,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };
        var created = await _repository.CreateAsync(entity);
        return ControllerResponseBuilder.Success(Map(created), "Created", HttpStatusCode.Created);
    }

    [HasPermission(Permissions.Payments.Update)]
    [CacheRemove(nameof(CacheKeys.PaymentsPattern))]
    [ValidationAspect(typeof(Validators.FluentValidation.Payments.UpdatePaymentRequestValidator))]
    public async Task<BaseControllerResponse<PaymentResponse>> UpdateAsync(Guid id, UpdatePaymentRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("PaymentNotFound", id);
        if (entity.PaymentStatus != Core.Enums.PaymentStatus.AWAITING.ToString() && request.PaymentStatus != entity.PaymentStatus)
            throw new BusinessRulesException("InvalidPaymentStatusTransition");
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        if (request.Amount.HasValue)
            entity.Amount = request.Amount.Value;
        if (!string.IsNullOrEmpty(request.Currency))
            entity.Currency = request.Currency;
        if (!string.IsNullOrEmpty(request.PaymentType))
            entity.PaymentType = request.PaymentType;
        entity.PaymentStatus = request.PaymentStatus;
        if (request.TxHash != null)
            entity.TxHash = request.TxHash;
        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;
        var updated = await _repository.UpdateAsync(entity);
        return ControllerResponseBuilder.Success(Map(updated));
    }

    [HasPermission(Permissions.Payments.Update)]
    [CacheRemove(nameof(CacheKeys.PaymentsPattern))]
    public async Task ConfirmPaymentAsync(Guid paymentId, string transactionHash)
    {
        var payment = await _repository.GetByIdAsync(paymentId);
        if (payment == null)
            throw new NotFoundException("PaymentNotFound", paymentId);

        var gateway = _gatewayResolver.Resolve(payment.PaymentType);
        var context = new PaymentContext
        {
            PaymentId = payment.Id,
            PaymentMethod = payment.PaymentType,
            Amount = payment.Amount
        };

        var isValid = await gateway.ConfirmPaymentAsync(context, transactionHash);
        if (!isValid)
            throw new BusinessRulesException("InvalidTransactionHash");

        payment.PaymentStatus = PaymentStatus.CONFIRMED.ToString();
        payment.TxHash = transactionHash;
        payment.UpdatedAt = DateTime.UtcNow;
        payment.UpdatedBy = _httpContextAccessor.HttpContext.GetUserId();
        await _repository.UpdateAsync(payment);
    }

    [HasPermission(Permissions.Payments.Delete)]
    [CacheRemove(nameof(CacheKeys.PaymentsPattern))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("PaymentNotFound", id);
        await _repository.SoftDeleteAsync(id);
        return ControllerResponseBuilder.Success();
    }

    private static PaymentResponse Map(PaymentEntity entity) => new()
    {
        Id = entity.Id,
        OrderId = entity.OrderId,
        BuyerId = entity.BuyerId,
        Amount = entity.Amount,
        Currency = entity.Currency,
        PaymentType = entity.PaymentType,
        PaymentStatus = entity.PaymentStatus,
        TxHash = entity.TxHash,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy
    };
}
