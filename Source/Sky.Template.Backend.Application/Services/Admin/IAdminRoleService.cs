using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Models;
using Sky.Template.Backend.Contract.Requests.Roles;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.RoleResponses;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Application.Validators.FluentValidation.Role;
using Sky.Template.Backend.Infrastructure.Entities.UserRole;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminRoleService
{
    #region CRUD Operations
    Task<BaseControllerResponse<RoleListResponse>> GetFilteredPaginatedRolesAsync(RoleFilterRequest request);
    Task<BaseControllerResponse<SingleRoleResponse>> GetRoleByIdAsync(int id);
    Task<BaseControllerResponse<SingleRoleResponse>> CreateRoleAsync(CreateRoleRequest request);
    Task<BaseControllerResponse<SingleRoleResponse>> UpdateRoleAsync(UpdateRoleRequest request);
    Task<BaseControllerResponse> DeleteRoleAsync(int id);
    Task<BaseControllerResponse> SoftDeleteRoleAsync(int id, string reason = "");
    #endregion

    #region Read-Interface
    Task<BaseControllerResponse<RolePageResponse>> GetAllRolesWithTotalUserCountAsync();
    Task<BaseControllerResponse<RoleDto>> GetRoleByIdWithTotalUserCountOrThrowAsync(int roleId);
    Task<BaseControllerResponse<GetUsersByRoleIdResponse>> GetAllUsersByRoleIdAsync(GetUsersByRoleRequest request);
    Task<BaseControllerResponse<UpdateUserRoleResponse>> UpdateUserRoleAsync(UpdateUserRoleRequest request);
    Task<BaseControllerResponse> AddPermissionToRoleAsync(AddPermissionToRoleRequest request);
    #endregion
}

public class AdminRoleService : IAdminRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRoleHelperService _userRoleHelperService;

    public AdminRoleService(IRoleRepository roleRepository, IPermissionRepository permissionRepository, IHttpContextAccessor httpContextAccessor, IUserRoleHelperService userRoleHelperService)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _httpContextAccessor = httpContextAccessor;
        _userRoleHelperService = userRoleHelperService;
    }

    #region CRUD Operations
    [HasPermission(Permissions.Roles.View)]
    public async Task<BaseControllerResponse<RoleListResponse>> GetFilteredPaginatedRolesAsync(RoleFilterRequest request)
    {
        var (roles, totalCount) = await _roleRepository.GetFilteredPaginatedAsync(request);

        if (!roles.Any())
        {
            throw new NotFoundException("RoleNotFound");
        }

        var roleList = roles.Select(r => new RoleDto()
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Status = r.Status,
            CreatedAt = r.CreatedAt,
            CreatedBy = r.CreatedBy,
            UpdatedAt = r.UpdatedAt,
            UpdatedBy = r.UpdatedBy,
            IsDeleted = r.IsDeleted,
            DeletedAt = r.DeletedAt,
            DeletedBy = r.DeletedBy,
            DeleteReason = r.DeleteReason,
            TotalUserCount = 0 // Bu değer ayrı bir sorgu ile alınabilir
        });

        return ControllerResponseBuilder.Success(new RoleListResponse()
        {
            Roles = new PaginatedData<RoleDto>()
            {
                TotalCount = totalCount,
                Items = roleList.ToList(),
                PageSize = roles.Count(),
                Page = request.Page,
                TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
            }
        });
    }

    [HasPermission(Permissions.Roles.View)]
    public async Task<BaseControllerResponse<SingleRoleResponse>> GetRoleByIdAsync(int id)
    {
        var role = await _roleRepository.GetByIdAsync(id);

        if (role == null)
        {
            throw new NotFoundException("RoleNotFoundWithId", id);
        }

        return ControllerResponseBuilder.Success(new SingleRoleResponse()
        {
            Role = new RoleDto()
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                Status = role.Status,
                CreatedAt = role.CreatedAt,
                CreatedBy = role.CreatedBy,
                UpdatedAt = role.UpdatedAt,
                UpdatedBy = role.UpdatedBy,
                IsDeleted = role.IsDeleted,
                DeletedAt = role.DeletedAt,
                DeletedBy = role.DeletedBy,
                DeleteReason = role.DeleteReason,
                TotalUserCount = 0
            }
        });
    }

    [HasPermission(Permissions.Roles.Create)]
    [ValidationAspect(typeof(CreateRoleRequestValidator))]
    [InvalidateCache(CacheKeys.RolesPattern)]
    public async Task<BaseControllerResponse<SingleRoleResponse>> CreateRoleAsync(CreateRoleRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var role = new RoleEntity()
        {
            Name = request.Name,
            Description = request.Description,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };

        // Tek insert işlemi - UnitOfWork gerekmez
        var createdRole = await _roleRepository.CreateAsync(role);

        return ControllerResponseBuilder.Success(new SingleRoleResponse()
        {
            Role = new RoleDto()
            {
                Id = createdRole.Id,
                Name = createdRole.Name,
                Description = createdRole.Description,
                Status = createdRole.Status,
                CreatedAt = createdRole.CreatedAt,
                CreatedBy = createdRole.CreatedBy,
                UpdatedAt = createdRole.UpdatedAt,
                UpdatedBy = createdRole.UpdatedBy,
                IsDeleted = createdRole.IsDeleted,
                DeletedAt = createdRole.DeletedAt,
                DeletedBy = createdRole.DeletedBy,
                DeleteReason = createdRole.DeleteReason,
                TotalUserCount = 0
            }
        }, "RoleCreatedSuccessfully");
    }

    [HasPermission(Permissions.Roles.Update)]
    [ValidationAspect(typeof(UpdateRoleRequestValidator))]
    [InvalidateCache(CacheKeys.RolesPattern)]
    public async Task<BaseControllerResponse<SingleRoleResponse>> UpdateRoleAsync(UpdateRoleRequest request)
    {
        var role = await _roleRepository.GetByIdAsync(request.Id);

        if (role == null)
        {
            throw new NotFoundException("RoleNotFoundWithId", request.Id);
        }

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        role.Name = request.Name;
        role.Description = request.Description;
        role.Status = request.Status;
        role.UpdatedAt = DateTime.UtcNow;
        role.UpdatedBy = userId;

        // Tek update işlemi - UnitOfWork gerekmez
        var updatedRole = await _roleRepository.UpdateAsync(role);

        return ControllerResponseBuilder.Success(new SingleRoleResponse()
        {
            Role = new RoleDto()
            {
                Id = updatedRole.Id,
                Name = updatedRole.Name,
                Description = updatedRole.Description,
                Status = updatedRole.Status,
                CreatedAt = updatedRole.CreatedAt,
                CreatedBy = updatedRole.CreatedBy,
                UpdatedAt = updatedRole.UpdatedAt,
                UpdatedBy = updatedRole.UpdatedBy,
                IsDeleted = updatedRole.IsDeleted,
                DeletedAt = updatedRole.DeletedAt,
                DeletedBy = updatedRole.DeletedBy,
                DeleteReason = updatedRole.DeleteReason,
                TotalUserCount = 0
            }
        }, "RoleUpdatedSuccessfully");
    }

    [HasPermission(Permissions.Roles.Delete)]
    [InvalidateCache(CacheKeys.RolesPattern)]
    public async Task<BaseControllerResponse> DeleteRoleAsync(int id)
    {
        var isDeleted = await _roleRepository.DeleteAsync(id);

        if (!isDeleted)
        {
            throw new NotFoundException("RoleNotFoundWithId", id);
        }

        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.Roles.Delete)]
    [InvalidateCache(CacheKeys.RolesPattern)]
    public async Task<BaseControllerResponse> SoftDeleteRoleAsync(int id, string reason = "")
    {
        var isDeleted = await _roleRepository.SoftDeleteAsync(id, reason);

        if (!isDeleted)
        {
            throw new NotFoundException("RoleNotFoundWithId", id);
        }

        return ControllerResponseBuilder.Success();
    }
    #endregion

    #region Read-Interface
    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.RolesPrefix), ExpirationInMinutes = 60)]
    [HasPermission(Permissions.Roles.View)]
    public async Task<BaseControllerResponse<RolePageResponse>> GetAllRolesWithTotalUserCountAsync()
    {
        var entities = await _roleRepository.GetAllRolesWithUserCountAsync();

        RolePageResponse response = new()
        {
            Roles = entities.Select(x => new RoleDto()
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                UpdatedAt = x.UpdatedAt,
                UpdatedBy = x.UpdatedBy,
                IsDeleted = x.IsDeleted,
                DeletedAt = x.DeletedAt,
                DeletedBy = x.DeletedBy,
                DeleteReason = x.DeleteReason,
                TotalUserCount = x.TotalUserCount
            }).ToList()
        };

        return ControllerResponseBuilder.Success(response);
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.RolesPrefix), ExpirationInMinutes = 60)]
    [HasPermission(Permissions.Roles.View)]
    public async Task<BaseControllerResponse<RoleDto>> GetRoleByIdWithTotalUserCountOrThrowAsync(int roleId)
    {
        var roleEntity = await _roleRepository.GetRoleByIdAsync(roleId);

        if (roleEntity == null)
        {
            throw new NotFoundException("RoleNotFoundWithId", roleId);
        }

        return ControllerResponseBuilder.Success(new RoleDto()
        {
            Id = roleEntity.Id,
            Name = roleEntity.Name,
            Description = roleEntity.Description,
            Status = roleEntity.Status,
            CreatedAt = roleEntity.CreatedAt,
            CreatedBy = roleEntity.CreatedBy,
            UpdatedAt = roleEntity.UpdatedAt,
            UpdatedBy = roleEntity.UpdatedBy,
            IsDeleted = roleEntity.IsDeleted,
            DeletedAt = roleEntity.DeletedAt,
            DeletedBy = roleEntity.DeletedBy,
            DeleteReason = roleEntity.DeleteReason,
            TotalUserCount = roleEntity.TotalUserCount
        });
    }

    [ValidationAspect(typeof(GetUsersByRoleRequestValidator))]
    [HasPermission(Permissions.Roles.View)]
    public async Task<BaseControllerResponse<GetUsersByRoleIdResponse>> GetAllUsersByRoleIdAsync(GetUsersByRoleRequest request)
    {
        var (entities, totalCount) = await _roleRepository.GetFilteredPaginatedUsersByRoleId(request);

        return ControllerResponseBuilder.Success(new GetUsersByRoleIdResponse
        {
            Users = new PaginatedData<Core.Models.User>()
            {
                TotalCount = totalCount,
                Items = entities.Select(x => new Core.Models.User()
                {
                    Id = x.Id,
                    Email = x.Email,
                    FirstName = x.Name,
                    LastName = x.Surname,
                    UserImagePath = x.ImagePath
                }).ToList(),
                PageSize = entities.Count(),
                Page = request.Page,
                TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
            }
        });
    }

    [ValidationAspect(typeof(UpdateUserRoleRequestValidator))]
    [HasPermission(Permissions.Users.RoleChange)]
    [InvalidateCache(CacheKeys.RolesPattern)]
    [HasPermission(Permissions.Roles.Update)]
    public async Task<BaseControllerResponse<UpdateUserRoleResponse>> UpdateUserRoleAsync(UpdateUserRoleRequest request)
    {
        return await _userRoleHelperService.UpdateUserRoleAsync(request);
    }

    [ValidationAspect(typeof(AddPermissionToRoleRequestValidator))]
    [HasPermission(Permissions.Roles.PermissionAdd)]
    [InvalidateCache(CacheKeys.RolesPattern)]
    public async Task<BaseControllerResponse> AddPermissionToRoleAsync(AddPermissionToRoleRequest request)
    {
        var role = await GetRoleByIdWithTotalUserCountOrThrowAsync(request.RoleId);
        var permission = await _permissionRepository.GetByIdAsync(request.PermissionId);
        if (permission == null)
            throw new NotFoundException("PermissionNotFoundWithId", request.PermissionId);

        var success = await _roleRepository.AddPermissionToRoleAsync(request.RoleId, request.PermissionId, _httpContextAccessor.HttpContext.GetUserId());
        if (!success)
            throw new BusinessRulesException("RolePermissionCreateFailed");

        return ControllerResponseBuilder.Success();
    }
    #endregion
}
