using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Application.Validators.FluentValidation.Resource;
using Sky.Template.Backend.Contract.Requests.Resources;
using Sky.Template.Backend.Contract.Responses.ResourceResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Infrastructure.Entities.System;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminResourceService
{
    Task<BaseControllerResponse<ResourceListResponse>> GetAllAsync();
    Task<BaseControllerResponse<SingleResourceResponse>> GetByCodeAsync(string code);
    Task<BaseControllerResponse<SingleResourceResponse>> CreateAsync(CreateResourceRequest request);
    Task<BaseControllerResponse<SingleResourceResponse>> UpdateAsync(string code, UpdateResourceRequest request);
    Task<BaseControllerResponse> DeleteAsync(string code);
}

public class AdminResourceService : IAdminResourceService
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const int CacheDuration = 60;

    public AdminResourceService(IResourceRepository resourceRepository, IHttpContextAccessor httpContextAccessor)
    {
        _resourceRepository = resourceRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.ResourcesPrefix), ExpirationInMinutes = CacheDuration)]
    public async Task<BaseControllerResponse<ResourceListResponse>> GetAllAsync()
    {
        var resources = await _resourceRepository.GetAllAsync();
        var response = new ResourceListResponse
        {
            Resources = resources.Select(r => new ResourceDto
            {
                Id = r.Id,
                Code = r.Code,
                Name = r.Name,
                Description = r.Description,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                CreatedBy = r.CreatedBy,
                UpdatedAt = r.UpdatedAt,
                UpdatedBy = r.UpdatedBy,
                DeletedBy = r.DeletedBy,
                DeletedAt = r.DeletedAt,
                DeleteReason = r.DeleteReason,
                Permissions = r.Permissions.Select(p => new ResourcesPermissionDto
                {
                    Code = p.Code,
                    Action = p.Action
                }).ToList()
            }).ToList(),
        };
        return ControllerResponseBuilder.Success(response);
    }

    public async Task<BaseControllerResponse<SingleResourceResponse>> GetByCodeAsync(string code)
    {
        var resource = await _resourceRepository.GetByCodeAsync(code);
        if (resource is null)
            throw new NotFoundException("ResourceNotFound", code);

        var dto = new ResourceDto
        {
            Id = resource.Id,
            Code = resource.Code,
            Name = resource.Name,
            Description = resource.Description,
            Status = resource.Status,
            CreatedAt = resource.CreatedAt,
            CreatedBy = resource.CreatedBy,
            UpdatedAt = resource.UpdatedAt,
            UpdatedBy = resource.UpdatedBy,
            DeletedBy = resource.DeletedBy,
            DeletedAt = resource.DeletedAt,
            DeleteReason = resource.DeleteReason,
            Permissions = resource.Permissions.Select(p => new ResourcesPermissionDto
            {
                Code = p.Code,
                Action = p.Action
            }).ToList(),
        };
        return ControllerResponseBuilder.Success(new SingleResourceResponse { Resource = dto });
    }

    [ValidationAspect(typeof(CreateResourceRequestValidator))]
    [InvalidateCache(CacheKeys.ResourcesPattern)]
    [InvalidateCache(nameof(CacheKeys.UserResourcesPattern))]
    public async Task<BaseControllerResponse<SingleResourceResponse>> CreateAsync(CreateResourceRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var entity = new ResourceEntity
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        var created = await _resourceRepository.CreateAsync(entity);
        var dto = new ResourceDto
        {
            Id = created.Id,
            Code = created.Code,
            Name = created.Name,
            Description = created.Description,
            Status = created.Status,
            CreatedAt = created.CreatedAt,
            CreatedBy = created.CreatedBy,
            UpdatedAt = created.UpdatedAt,
            UpdatedBy = created.UpdatedBy,
            DeletedBy = created.DeletedBy,
            DeletedAt = created.DeletedAt,
            DeleteReason = created.DeleteReason,
            Permissions = created.Permissions.Select(p => new ResourcesPermissionDto
            {
                Code = p.Code,
                Action = p.Action
            }).ToList()
        };
        return ControllerResponseBuilder.Success(new SingleResourceResponse { Resource = dto }, "ResourceCreatedSuccessfully");
    }

    [ValidationAspect(typeof(UpdateResourceRequestValidator))]
    [InvalidateCache(CacheKeys.ResourcesPattern)]
    [InvalidateCache(nameof(CacheKeys.UserResourcesPattern))]
    public async Task<BaseControllerResponse<SingleResourceResponse>> UpdateAsync(string code, UpdateResourceRequest request)
    {
        var entity = await _resourceRepository.GetByCodeAsync(code);
        if (entity is null)
            throw new NotFoundException("ResourceNotFound", code);

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;
        var updated = await _resourceRepository.UpdateAsync(entity);
        var dto = new ResourceDto
        {
            Id = updated.Id,
            Code = updated.Code,
            Name = updated.Name,
            Description = updated.Description,
            Status = updated.Status,
            CreatedAt = updated.CreatedAt,
            CreatedBy = updated.CreatedBy,
            UpdatedAt = updated.UpdatedAt,
            UpdatedBy = updated.UpdatedBy,
            DeletedBy = updated.DeletedBy,
            DeletedAt = updated.DeletedAt,
            DeleteReason = updated.DeleteReason,
            Permissions = updated.Permissions.Select(p => new ResourcesPermissionDto
            {
                Code = p.Code,
                Action = p.Action
            }).ToList()
        };
        return ControllerResponseBuilder.Success(new SingleResourceResponse { Resource = dto }, "ResourceUpdatedSuccessfully");
    }

    [InvalidateCache(CacheKeys.ResourcesPattern)]
    [InvalidateCache(nameof(CacheKeys.UserResourcesPattern))]
    public async Task<BaseControllerResponse> DeleteAsync(string code)
    {
        var success = await _resourceRepository.DeleteAsync(code);
        if (!success)
            throw new NotFoundException("ResourceNotFound", code);
        return ControllerResponseBuilder.Success();
    }
}
