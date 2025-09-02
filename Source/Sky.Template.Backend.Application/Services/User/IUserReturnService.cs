using Sky.Template.Backend.Core.Constants;
using System.Net;
using System;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.Returns;
using Sky.Template.Backend.Contract.Responses.ReturnResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Core.Enums;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using System.Linq;

namespace Sky.Template.Backend.Application.Services.User;

public interface IUserReturnService
{
    Task<BaseControllerResponse<ReturnResponse>> CreateReturnAsync(CreateReturnRequest request);
    Task<BaseControllerResponse<ReturnResponse>> GetReturnByIdAsync(Guid id);
    Task<BaseControllerResponse<IEnumerable<ReturnResponse>>> GetMyReturnsAsync();
    Task<BaseControllerResponse> CancelReturnAsync(Guid returnId);
}

public class UserReturnService : IUserReturnService
{
    private readonly IReturnRepository _returnRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserReturnService(IReturnRepository returnRepository, IHttpContextAccessor httpContextAccessor)
    {
        _returnRepository = returnRepository;
        _httpContextAccessor = httpContextAccessor;
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

    [HasPermission(Permissions.Returns.Read)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<ReturnResponse>> GetReturnByIdAsync(Guid id)
    {
        var entity = await _returnRepository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("ReturnNotFound", id);
        var response = MapToResponse(entity);
        return ControllerResponseBuilder.Success(response);
    }

    [HasPermission(Permissions.Returns.Read)]
    public async Task<BaseControllerResponse<IEnumerable<ReturnResponse>>> GetMyReturnsAsync()
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var data = await _returnRepository.GetAllAsync();
        var filtered = data.Where(r => r.BuyerId == userId).Select(MapToResponse);
        return ControllerResponseBuilder.Success(filtered);
    }

    [HasPermission(Permissions.Returns.Delete)]
    [EnsureUserIsValid(new[] { "returnId" })]
    public async Task<BaseControllerResponse> CancelReturnAsync(Guid returnId)
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var entity = await _returnRepository.GetByIdAsync(returnId);
        if (entity == null || entity.BuyerId != userId)
            throw new NotFoundException("ReturnNotFound", returnId);
        if (entity.Status != ReturnStatus.PENDING.ToString())
            throw new BusinessRulesException("ReturnCannotBeCanceled");
        await _returnRepository.SoftDeleteAsync(returnId);
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

