using Sky.Template.Backend.Core.Constants;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Core.Models;
using Sky.Template.Backend.Infrastructure.Entities.User;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Contract.Requests.Users;
using Sky.Template.Backend.Contract.Requests.Roles;
using Sky.Template.Backend.Contract.Responses.UserResponses;
using Sky.Template.Backend.Contract.Responses.RoleResponses;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminUserService
{
    Task<BaseControllerResponse<FilteredUsersResponse>> GetAllUsersAsync(UsersFilterRequest request);
    Task<BaseControllerResponse<SingleUserResponse>> UpdateUserAsync(UpdateUserRequest request);
    Task<BaseControllerResponse> SoftDeleteUserAsync(Guid id, string reason = "");
    Task<BaseControllerResponse<UpdateUserRoleResponse>> ChangeUserRoleAsync(UpdateUserRoleRequest request);
}

public class AdminUserService : IAdminUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserRoleHelperService _userRoleHelperService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdminUserService(IUserRepository userRepository, IUserRoleHelperService userRoleHelperService, IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _userRoleHelperService = userRoleHelperService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<BaseControllerResponse<FilteredUsersResponse>> GetAllUsersAsync(UsersFilterRequest request)
    {
        var (users, totalCount) = await _userRepository.GetAllUsersWithFilterAsync(request);
        if (!users.Any())
            throw new NotFoundException("UserNotFound");

        var userList = users.Select(u => new UserWithRoleDto
        {
            Id = u.Id,
            FirstName = u.Name,
            LastName = u.Surname,
            Email = u.Email,
            UserImagePath = u.ImagePath,
            CustomFieldList = u.CustomFields,
            Role = new Role
            {
                Id = u.RoleId,
                Name = u.RoleName,
                Description = u.RoleDescription
            }
        });

        return ControllerResponseBuilder.Success(new FilteredUsersResponse
        {
            Users = new PaginatedData<UserWithRoleDto>
            {
                TotalCount = totalCount,
                Items = userList.ToList(),
                PageSize = users.Count(),
                Page = request.Page,
                TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
            }
        });
    }

    [HasPermission(Permissions.Users.Update)]
    public async Task<BaseControllerResponse<SingleUserResponse>> UpdateUserAsync(UpdateUserRequest request)
    {
        var existing = await _userRepository.GetUserWithRoleByIdAsync(request.Id);
        if (existing == null)
            throw new NotFoundException("UserNotFoundWithId", request.Id);

        var entity = new UserEntity
        {
            Id = request.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Status = request.Status,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = _httpContextAccessor.HttpContext.GetUserId()
        };

        var updated = await _userRepository.UpdateUserAsync(entity);

        return ControllerResponseBuilder.Success(new SingleUserResponse
        {
            User = new UserWithRoleDto
            {
                Id = updated.Id,
                FirstName = updated.FirstName,
                LastName = updated.LastName,
                Email = updated.Email,
                UserImagePath = updated.ImagePath,
                Role = new Role
                {
                    Id = existing.RoleId,
                    Name = existing.RoleName,
                    Description = existing.RoleDescription
                }
            }
        });
    }

    [HasPermission(Permissions.Users.SoftDelete)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> SoftDeleteUserAsync(Guid id, string reason = "")
    {
        var user = await _userRepository.GetUserWithRoleByIdAsync(id);
        if (user == null)
            throw new NotFoundException("UserNotFoundWithId", id);

        var success = await _userRepository.SoftDeleteUserAsync(id, reason);
        if (!success)
            throw new BusinessRulesException("UserSoftDeleteFailed");

        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.Users.RoleChange)]
    public async Task<BaseControllerResponse<UpdateUserRoleResponse>> ChangeUserRoleAsync(UpdateUserRoleRequest request)
        => await _userRoleHelperService.UpdateUserRoleAsync(request);
}
