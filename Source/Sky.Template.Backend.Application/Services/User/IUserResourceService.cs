using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Responses.ResourceResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.User;

public interface IUserResourceService
{
    Task<BaseControllerResponse<ResourceListResponse>> GetCurrentUserResourcesAsync();
}

public class UserResourceService : IUserResourceService
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const int CacheDuration = 60;

    public UserResourceService(IResourceRepository resourceRepository, IHttpContextAccessor httpContextAccessor)
    {
        _resourceRepository = resourceRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.UserResourcesPrefix), ExpirationInMinutes = CacheDuration)]
    public Task<BaseControllerResponse<ResourceListResponse>> GetCurrentUserResourcesAsync()
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        return GetUserResourcesAsync(userId);
    }

    private async Task<BaseControllerResponse<ResourceListResponse>> GetUserResourcesAsync(Guid userId)
    {
        var resources = await _resourceRepository.GetByUserAsync(userId);
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
}
