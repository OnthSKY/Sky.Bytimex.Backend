using Sky.Template.Backend.Core.Constants;
using System.Net;
using System;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.Returns;
using Sky.Template.Backend.Contract.Responses.ReturnResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Application.Services;
using Sky.Template.Backend.Contract.Requests.AuditLog;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using System.Linq;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminReturnService
{
    Task<BaseControllerResponse<ReturnListPaginatedResponse>> GetReturnsAsync(ReturnFilterRequest request);
    Task<BaseControllerResponse<ReturnResponse>> UpdateReturnStatusAsync(Guid id, UpdateReturnStatusRequest request);
    Task<BaseControllerResponse<ReturnResponse>> CreateReturnAsync(CreateReturnRequest request);
    Task<BaseControllerResponse> DeleteReturnAsync(Guid id);
}

public class AdminReturnService : IAdminReturnService
{
    private readonly IReturnRepository _returnRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;

    public AdminReturnService(IReturnRepository returnRepository, IHttpContextAccessor httpContextAccessor, IAuditLogService auditLogService)
    {
        _returnRepository = returnRepository;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
    }

    [HasPermission(Permissions.Returns.Read)]
    public async Task<BaseControllerResponse<ReturnListPaginatedResponse>> GetReturnsAsync(ReturnFilterRequest request)
    {
        var (data, totalCount) = await _returnRepository.GetFilteredPaginatedAsync(request);
        var mapped = data.Select(MapToResponse).ToList();
        var paginated = new PaginatedData<ReturnResponse>
        {
            Items = mapped,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
        return ControllerResponseBuilder.Success(new ReturnListPaginatedResponse { Returns = paginated });
    }

    [HasPermission(Permissions.Returns.Create)]
    [ValidationAspect(typeof(Validators.FluentValidation.Returns.CreateReturnRequestValidator))]
    public async Task<BaseControllerResponse<ReturnResponse>> CreateReturnAsync(CreateReturnRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var entity = new ReturnEntity
        {
            OrderId = request.OrderId,
            OrderDetailId = request.OrderDetailId ?? request.Items.FirstOrDefault(),
            BuyerId = request.BuyerId,
            Reason = request.Reason,
            Status = "PENDING",
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };
        var created = await _returnRepository.CreateAsync(entity);
        var response = MapToResponse(created);
        return ControllerResponseBuilder.Success(response, "ReturnCreatedSuccessfully", HttpStatusCode.Created);
    }

    [HasPermission(Permissions.Returns.Update)]
    [ValidationAspect(typeof(Validators.FluentValidation.Returns.UpdateReturnStatusRequestValidator))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<ReturnResponse>> UpdateReturnStatusAsync(Guid id, UpdateReturnStatusRequest request)
    {
        var entity = await _returnRepository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("ReturnNotFound", id);

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        entity.Status = request.Status;
        entity.ProcessedAt = DateTime.UtcNow;
        entity.ProcessedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;

        var updated = await _returnRepository.UpdateAsync(entity);
        var response = MapToResponse(updated);
        return ControllerResponseBuilder.Success(response);
    }

    [HasPermission(Permissions.Returns.Delete)]
    public async Task<BaseControllerResponse> DeleteReturnAsync(Guid id)
    {
        var entity = await _returnRepository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("ReturnNotFound", id);
        await _returnRepository.DeleteAsync(id);
        await _auditLogService.Execute(new AuditLogParameters
        {
            ActivityId = Guid.NewGuid(),
            EventName = "ReturnDeleted",
            ModuleName = "Return",
            RequestTime = DateTime.UtcNow,
            ResponseTime = DateTime.UtcNow,
            RequestUrl = string.Empty,
            PageUrl = string.Empty,
            RequestBody = string.Empty,
            ResponseBody = string.Empty,
            User = string.Empty,
            Device = string.Empty,
            Browser = string.Empty,
            Application = string.Empty,
            DeviceFamily = string.Empty,
            DeviceType = string.Empty,
            UserId = 0
        });
        return ControllerResponseBuilder.Success();
    }

    private static ReturnResponse MapToResponse(ReturnEntity entity) => new()
    {
        Id = entity.Id,
        OrderId = entity.OrderId,
        OrderDetailId = entity.OrderDetailId,
        BuyerId = entity.BuyerId,
        Reason = entity.Reason,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy,
        ProcessedAt = entity.ProcessedAt,
        ProcessedBy = entity.ProcessedBy,
        Items = entity.OrderDetailId.HasValue ? new[] { entity.OrderDetailId.Value } : Array.Empty<Guid>()
    };
}

