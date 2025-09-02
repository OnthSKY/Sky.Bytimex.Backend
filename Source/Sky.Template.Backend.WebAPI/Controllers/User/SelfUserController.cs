using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.AdminUsers;
using Sky.Template.Backend.Contract.Requests.Users;
using Sky.Template.Backend.Core.Extensions;

namespace Sky.Template.Backend.WebAPI.Controllers.User;

[ApiController]
[Route("api/self")]
[ApiVersion("1.0")]
public class SelfUserController : UserBaseController
{
    private readonly IUserService _userService;
    public SelfUserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("v{version:apiVersion}/profile")]
    public async Task<IActionResult> Profile()
        => await HandleServiceResponseAsync(() => _userService.GetUserDtoByIdAsync(HttpContext.GetUserId()));

    [HttpPut("v{version:apiVersion}/profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] SelfUpdateProfileRequest request)
    {
        var userId = HttpContext.GetUserId();
        var update = new UpdateUserRequest
        {
            Id = userId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = string.Empty,
            Status = "ACTIVE"
        };
        return await HandleServiceResponseAsync(() => _userService.UpdateUserAsync(update));
    }
}
