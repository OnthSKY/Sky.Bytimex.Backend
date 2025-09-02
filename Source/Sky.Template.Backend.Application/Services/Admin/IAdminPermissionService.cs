using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Application.Validators.FluentValidation.Permission;
using Sky.Template.Backend.Contract.Requests.Permissions;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.PermissionResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities.System;
using Sky.Template.Backend.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminPermissionService
{
    #region CRUD Operations
    Task<BaseControllerResponse<PermissionListResponse>> GetFilteredPaginatedPermissionsAsync(PermissionFilterRequest request);
    Task<BaseControllerResponse<SinglePermissionResponse>> GetPermissionByIdAsync(int id);
    Task<BaseControllerResponse<SinglePermissionResponse>> CreatePermissionAsync(CreatePermissionRequest request);
    Task<BaseControllerResponse<SinglePermissionResponse>> UpdatePermissionAsync(UpdatePermissionRequest request);
    Task<BaseControllerResponse> DeletePermissionAsync(int id);
    Task<BaseControllerResponse> SoftDeletePermissionAsync(int id, string reason = "");
    #endregion
}


    public class AdminPermissionService : IAdminPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminPermissionService(IPermissionRepository permissionRepository, IHttpContextAccessor httpContextAccessor)
    {
        _permissionRepository = permissionRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    #region CRUD Operations
    [HasPermission(Permissions.PermissionModule.View)]
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.PermissionsPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<PermissionListResponse>> GetFilteredPaginatedPermissionsAsync(PermissionFilterRequest request)
    {
        var (permissions, totalCount) = await _permissionRepository.GetFilteredPaginatedAsync(request);

        if (!permissions.Any())
        {
            throw new NotFoundException("PermissionNotFound");
        }

        var permissionList = permissions.Select(p => new PermissionDto()
        {
            Id = p.Id,
            Code = p.Code,
            Name = p.Name,
            Description = p.Description,
            ResourceId = p.ResourceId,
            Action = p.Action,
            IsDeleted = p.IsDeleted,
            CreatedAt = p.CreatedAt,
            CreatedBy = p.CreatedBy,
            UpdatedAt = p.UpdatedAt,
            UpdatedBy = p.UpdatedBy,
            DeletedAt = p.DeletedAt,
            DeletedBy = p.DeletedBy,
            DeleteReason = p.DeleteReason
        });

        return ControllerResponseBuilder.Success(new PermissionListResponse()
        {
            Permissions = new PaginatedData<PermissionDto>()
            {
                TotalCount = totalCount,
                Items = permissionList.ToList(),
                PageSize = permissions.Count(),
                Page = request.Page,
                TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
            }
        });
    }

    [HasPermission(Permissions.PermissionModule.View)]
    public async Task<BaseControllerResponse<SinglePermissionResponse>> GetPermissionByIdAsync(int id)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);

        if (permission == null)
        {
            throw new NotFoundException("PermissionNotFoundWithId", id);
        }

        return ControllerResponseBuilder.Success(new SinglePermissionResponse()
        {
            Permission = new PermissionDto()
            {
                Id = permission.Id,
                Code = permission.Code,
                Name = permission.Name,
                Description = permission.Description,
                ResourceId = permission.ResourceId,
                Action = permission.Action,
                IsDeleted = permission.IsDeleted,
                CreatedAt = permission.CreatedAt,
                CreatedBy = permission.CreatedBy,
                UpdatedAt = permission.UpdatedAt,
                UpdatedBy = permission.UpdatedBy,
                DeletedAt = permission.DeletedAt,
                DeletedBy = permission.DeletedBy,
                DeleteReason = permission.DeleteReason
            }
        });
    }

    [HasPermission(Permissions.PermissionModule.Create)]
    [ValidationAspect(typeof(CreatePermissionRequestValidator))]
    [InvalidateCache(CacheKeys.PermissionsPattern)]
    public async Task<BaseControllerResponse<SinglePermissionResponse>> CreatePermissionAsync(CreatePermissionRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var permission = new PermissionEntity()
        {
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };

        // Tek insert işlemi - UnitOfWork gerekmez
        var createdPermission = await _permissionRepository.CreateAsync(permission);

        return ControllerResponseBuilder.Success(new SinglePermissionResponse()
        {
            Permission = new PermissionDto()
            {
                Id = createdPermission.Id,
                Code = createdPermission.Code,
                Name = createdPermission.Name,
                Description = createdPermission.Description,
                ResourceId = createdPermission.ResourceId,
                Action = createdPermission.Action,
                IsDeleted = createdPermission.IsDeleted,
                CreatedAt = createdPermission.CreatedAt,
                CreatedBy = createdPermission.CreatedBy,
                UpdatedAt = createdPermission.UpdatedAt,
                UpdatedBy = createdPermission.UpdatedBy,
                DeletedAt = createdPermission.DeletedAt,
                DeletedBy = createdPermission.DeletedBy,
                DeleteReason = createdPermission.DeleteReason
            }
        }, "PermissionCreatedSuccessfully");
    }

    [HasPermission(Permissions.PermissionModule.Update)]
    [ValidationAspect(typeof(UpdatePermissionRequestValidator))]
    [InvalidateCache(CacheKeys.PermissionsPattern)]
    public async Task<BaseControllerResponse<SinglePermissionResponse>> UpdatePermissionAsync(UpdatePermissionRequest request)
    {
        var permission = await _permissionRepository.GetByIdAsync(request.Id);

        if (permission == null)
        {
            throw new NotFoundException("PermissionNotFoundWithId", request.Id);
        }

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        permission.Name = request.Name;
        permission.Description = request.Description;
        permission.UpdatedAt = DateTime.UtcNow;
        permission.UpdatedBy = userId;

        // Tek update işlemi - UnitOfWork gerekmez
        var updatedPermission = await _permissionRepository.UpdateAsync(permission);

        return ControllerResponseBuilder.Success(new SinglePermissionResponse()
        {
            Permission = new PermissionDto()
            {
                Id = updatedPermission.Id,
                Code = updatedPermission.Code,
                Name = updatedPermission.Name,
                Description = updatedPermission.Description,
                ResourceId = updatedPermission.ResourceId,
                Action = updatedPermission.Action,
                IsDeleted = updatedPermission.IsDeleted,
                CreatedAt = updatedPermission.CreatedAt,
                CreatedBy = updatedPermission.CreatedBy,
                UpdatedAt = updatedPermission.UpdatedAt,
                UpdatedBy = updatedPermission.UpdatedBy,
                DeletedAt = updatedPermission.DeletedAt,
                DeletedBy = updatedPermission.DeletedBy,
                DeleteReason = updatedPermission.DeleteReason
            }
        }, "PermissionUpdatedSuccessfully");
    }

    [HasPermission(Permissions.PermissionModule.Delete)]
    [InvalidateCache(CacheKeys.PermissionsPattern)]
    public async Task<BaseControllerResponse> DeletePermissionAsync(int id)
    {
        var isDeleted = await _permissionRepository.DeleteAsync(id);

        if (!isDeleted)
        {
            throw new NotFoundException("PermissionNotFoundWithId", id);
        }

        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.PermissionModule.Delete)]
    [InvalidateCache(CacheKeys.PermissionsPattern)]
    public async Task<BaseControllerResponse> SoftDeletePermissionAsync(int id, string reason = "")
    {
        var isDeleted = await _permissionRepository.SoftDeleteAsync(id, reason);

        if (!isDeleted)
        {
            throw new NotFoundException("PermissionNotFoundWithId", id);
        }

        return ControllerResponseBuilder.Success();
    }
    #endregion
}