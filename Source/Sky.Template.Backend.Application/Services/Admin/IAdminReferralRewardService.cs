using Sky.Template.Backend.Core.Constants;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Application.Validators.FluentValidation.ReferralRewards;
using Sky.Template.Backend.Contract.Requests.ReferralRewards;
using Sky.Template.Backend.Contract.Responses.ReferralRewardResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminReferralRewardService
{
    Task<BaseControllerResponse<ReferralRewardResponse>> CreateAsync(CreateReferralRewardRequest request);
    Task<BaseControllerResponse<ReferralRewardResponse>> UpdateAsync(Guid id, UpdateReferralRewardRequest request);
    Task<BaseControllerResponse> DeleteAsync(Guid id);
    Task<BaseControllerResponse<ReferralRewardResponse>> GetByIdAsync(Guid id);
    Task<BaseControllerResponse<List<ReferralRewardResponse>>> GetListAsync(Guid? referrerUserId = null, Guid? referredUserId = null, string? status = null);
}

public class AdminReferralRewardService : IAdminReferralRewardService
{
    private readonly IReferralRewardRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public AdminReferralRewardService(IReferralRewardRepository repository, IHttpContextAccessor httpContextAccessor, IMapper mapper)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    [HasPermission(Permissions.ReferralRewards.Create)]
    [ValidationAspect(typeof(CreateReferralRewardRequestValidator))]
    public async Task<BaseControllerResponse<ReferralRewardResponse>> CreateAsync(CreateReferralRewardRequest request)
    {
        var entity = _mapper.Map<ReferralRewardEntity>(request);
        entity.CreatedBy = _httpContextAccessor.HttpContext.GetUserId();
        var created = await _repository.CreateAsync(entity);
        return ControllerResponseBuilder.Success(_mapper.Map<ReferralRewardResponse>(created));
    }

    [HasPermission(Permissions.ReferralRewards.Update)]
    [ValidationAspect(typeof(UpdateReferralRewardRequestValidator))]
    public async Task<BaseControllerResponse<ReferralRewardResponse>> UpdateAsync(Guid id, UpdateReferralRewardRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("ReferralRewardNotFound", id);

        _mapper.Map(request, entity);
        entity.Id = id;
        entity.UpdatedBy = _httpContextAccessor.HttpContext.GetUserId();
        var updated = await _repository.UpdateAsync(entity);
        return ControllerResponseBuilder.Success(_mapper.Map<ReferralRewardResponse>(updated));
    }

    [HasPermission(Permissions.ReferralRewards.Delete)]
    public async Task<BaseControllerResponse> DeleteAsync(Guid id)
    {
        await _repository.SoftDeleteAsync(id);
        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.ReferralRewards.View)]
    public async Task<BaseControllerResponse<ReferralRewardResponse>> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("ReferralRewardNotFound", id);
        return ControllerResponseBuilder.Success(_mapper.Map<ReferralRewardResponse>(entity));
    }

    [HasPermission(Permissions.ReferralRewards.View)]
    public async Task<BaseControllerResponse<List<ReferralRewardResponse>>> GetListAsync(Guid? referrerUserId = null, Guid? referredUserId = null, string? status = null)
    {
        var list = await _repository.GetListAsync(referrerUserId, referredUserId, status);
        return ControllerResponseBuilder.Success(_mapper.Map<List<ReferralRewardResponse>>(list));
    }
}
