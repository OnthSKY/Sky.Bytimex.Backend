using System;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.Roles;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.RoleResponses;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Core.Models;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Application.Services.User;

namespace Sky.Template.Backend.Application.Services;

public interface IUserRoleHelperService
{
    Task<BaseControllerResponse<UpdateUserRoleResponse>> UpdateUserRoleAsync(UpdateUserRoleRequest request);
}

public class UserRoleHelperService : IUserRoleHelperService
{
    private readonly Lazy<IUserService> _userService;
    private readonly Lazy<IAdminRoleService> _roleService;
    private readonly IRoleRepository _roleRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserRoleHelperService(
        Lazy<IUserService> userService,
        Lazy<IAdminRoleService> roleService,
        IRoleRepository roleRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _userService = userService;
        _roleService = roleService;
        _roleRepository = roleRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<BaseControllerResponse<UpdateUserRoleResponse>> UpdateUserRoleAsync(UpdateUserRoleRequest request)
    {
        var userResponse = await _userService.Value.GetUserDtoByIdOrThrowAsync(request.UserId);
        var roleResponse = await _roleService.Value.GetRoleByIdWithTotalUserCountOrThrowAsync(request.RoleId);

        var isUpdated = await _roleRepository.UpdateUserRoleAsync(request, _httpContextAccessor.HttpContext.GetUserId());

        if (!isUpdated)
        {
            throw new BusinessRulesException("UserRoleUpdateFailed");
        }

        return ControllerResponseBuilder.Success(new UpdateUserRoleResponse
        {
            UserId = userResponse.Data.User.Id,
            UserName = userResponse.Data.User.FirstName + " " + userResponse.Data.User.LastName,
            RoleId = roleResponse.Data.Id,
            RoleName = roleResponse.Data.Name,
        });
    }
}
