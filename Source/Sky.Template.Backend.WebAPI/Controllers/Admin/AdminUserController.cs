using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Users;
using Sky.Template.Backend.Contract.Requests.AdminUsers;
namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[ApiVersion("1.0")]
public class AdminUserController : AdminBaseController
{
    private readonly IAdminUserService _userService;
    public AdminUserController(IAdminUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetAllUsers([FromQuery] UsersFilterRequest request)
        => await HandleServiceResponseAsync(() => _userService.GetAllUsersAsync(request));

    [HttpPut("v{version:apiVersion}")]
    public async Task<IActionResult> UpdateUsers([FromBody] AdminUpdateUserRequest request)
        => await HandleServiceResponseAsync(() => _userService.UpdateUserAsync(new UpdateUserRequest
        {
            Id = request.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Status = request.Status
        }));

    [HttpDelete("v{version:apiVersion}/{id}/soft")]
    public async Task<IActionResult> SoftDelete(Guid id, [FromQuery] string? reason = "")
        => await HandleServiceResponseAsync(() => _userService.SoftDeleteUserAsync(id, reason ?? string.Empty));
}
