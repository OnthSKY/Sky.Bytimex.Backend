using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Roles;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/roles")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class RoleController : AdminBaseController
{
    private readonly IAdminRoleService _roleService;

    public RoleController(IAdminRoleService roleService)
    {
        _roleService = roleService;
    }

    #region CRUD Operations
    [HttpGet("v{version:apiVersion}/all")]
    public async Task<IActionResult> GetFilteredPaginatedRoles([FromQuery] RoleFilterRequest request)
    {
        return await HandleServiceResponseAsync(() => _roleService.GetFilteredPaginatedRolesAsync(request));
    }

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetRoleById([FromRoute] int id)
    {
        return await HandleServiceResponseAsync(() => _roleService.GetRoleByIdAsync(id));
    }

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        return await HandleServiceResponseAsync(() => _roleService.CreateRoleAsync(request));
    }

    [HttpPut("v{version:apiVersion}")]
    public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequest request)
    {
        return await HandleServiceResponseAsync(() => _roleService.UpdateRoleAsync(request));
    }

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> DeleteRole([FromRoute]int id)
    {
        return await HandleServiceResponseAsync(() => _roleService.DeleteRoleAsync(id));
    }

    [HttpDelete("v{version:apiVersion}/{id}/soft")]
    public async Task<IActionResult> SoftDeleteRole([FromRoute] int id, [FromQuery] string reason = "")
    {
        return await HandleServiceResponseAsync(() => _roleService.SoftDeleteRoleAsync(id, reason));
    }
    #endregion

    #region Existing Operations
    [HttpGet("v{version:apiVersion}/with-user-count")]
    public async Task<IActionResult> GetAllRolesWithTotalUserCount()
    {
        return await HandleServiceResponseAsync(() => _roleService.GetAllRolesWithTotalUserCountAsync());
    }

    [HttpGet("v{version:apiVersion}/{id}/with-user-count")]
    public async Task<IActionResult> GetRoleByIdWithTotalUserCount([FromRoute] int id)
    {
        return await HandleServiceResponseAsync(() => _roleService.GetRoleByIdWithTotalUserCountOrThrowAsync(id));
    }

    [HttpGet("v{version:apiVersion}/{roleId}/users")]
    public async Task<IActionResult> GetAllUsersByRoleId(int roleId, [FromQuery] GetUsersByRoleRequest request)
    {
        request.RoleId = roleId;
        return await HandleServiceResponseAsync(() => _roleService.GetAllUsersByRoleIdAsync(request));
    }

    [HttpPut("v{version:apiVersion}/user-role")]
    public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleRequest request)
    {
        return await HandleServiceResponseAsync(() => _roleService.UpdateUserRoleAsync(request));
    }

    [HttpPost("v{version:apiVersion}/{roleId}/permissions/{permissionId}")]
    public async Task<IActionResult> AddPermissionToRole([FromRoute]int roleId, [FromRoute]int permissionId)
    {
        var request = new AddPermissionToRoleRequest { RoleId = roleId, PermissionId = permissionId };
        return await HandleServiceResponseAsync(() => _roleService.AddPermissionToRoleAsync(request));
    }
    #endregion
}

