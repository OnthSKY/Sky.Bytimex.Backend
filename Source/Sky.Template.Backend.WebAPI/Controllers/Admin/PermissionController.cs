using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Permissions;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/permissions")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class PermissionController : AdminBaseController
{
    private readonly IAdminPermissionService _permissionService;

    public PermissionController(IAdminPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    #region CRUD Operations
    [HttpGet("v{version:apiVersion}/all")]
    public async Task<IActionResult> GetFilteredPaginatedPermissions([FromQuery] PermissionFilterRequest request)
    {
        return await HandleServiceResponseAsync(() => _permissionService.GetFilteredPaginatedPermissionsAsync(request));
    }

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetPermissionById(int id)
    {
        return await HandleServiceResponseAsync(() => _permissionService.GetPermissionByIdAsync(id));
    }

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionRequest request)
    {
        return await HandleServiceResponseAsync(() => _permissionService.CreatePermissionAsync(request));
    }

    [HttpPut("v{version:apiVersion}")]
    public async Task<IActionResult> UpdatePermission([FromBody] UpdatePermissionRequest request)
    {
        return await HandleServiceResponseAsync(() => _permissionService.UpdatePermissionAsync(request));
    }

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> DeletePermission(int id)
    {
        return await HandleServiceResponseAsync(() => _permissionService.DeletePermissionAsync(id));
    }

    [HttpDelete("v{version:apiVersion}/{id}/soft")]
    public async Task<IActionResult> SoftDeletePermission(int id, [FromQuery] string reason = "")
    {
        return await HandleServiceResponseAsync(() => _permissionService.SoftDeletePermissionAsync(id, reason));
    }
    #endregion
} 