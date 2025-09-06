using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Users;
using Sky.Template.Backend.Contract.Responses.UserResponses;
using Sky.Template.Backend.Core.Extensions;

namespace Sky.Template.Backend.WebAPI.Controllers.User;

[Authorize]
[ApiController]
[Route("api/users")]
[ApiVersion("1.0")]
public class SelfUserController : UserBaseController
{
    private readonly IUserService _userService;
    public SelfUserController(IUserService userService) => _userService = userService;

    [HttpGet("v{version:apiVersion}/profile")]
    public async Task<IActionResult> GetProfile()
        => await HandleServiceResponseAsync(() => _userService.GetSelfProfileAsync(HttpContext.GetUserId()));

    [HttpPut("v{version:apiVersion}/profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] SelfUpdateProfileRequest request)
        => await HandleServiceResponseAsync(() => _userService.UpdateSelfProfileAsync(HttpContext.GetUserId(), request));

    [HttpGet("v{version:apiVersion}/permissions")]
    public async Task<IActionResult> GetPermissions()
        => await HandleServiceResponseAsync(() => _userService.GetSelfPermissionsAsync(HttpContext.GetUserId()));

    [HttpGet("v{version:apiVersion}/addresses")]
    public async Task<IActionResult> GetAddresses()
        => await HandleServiceResponseAsync(() => _userService.GetSelfAddressesAsync(HttpContext.GetUserId()));

    [HttpGet("v{version:apiVersion}/sessions")]
    public async Task<IActionResult> GetSessions()
        => await HandleServiceResponseAsync(() => _userService.GetSelfSessionsAsync(HttpContext.GetUserId()));

    [HttpDelete("v{version:apiVersion}/sessions/{sessionId:guid}")]
    public async Task<IActionResult> RevokeSession(Guid sessionId)
        => await HandleServiceResponseAsync(() => _userService.RevokeSelfSessionAsync(HttpContext.GetUserId(), sessionId));

    [HttpPut("v{version:apiVersion}/notifications")]
    public async Task<IActionResult> UpdateNotifications([FromBody] NotificationSettingsDto request)
        => await HandleServiceResponseAsync(() => _userService.UpdateSelfNotificationsAsync(HttpContext.GetUserId(), request));

    [HttpPut("v{version:apiVersion}/preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UserPreferencesDto request)
        => await HandleServiceResponseAsync(() => _userService.UpdateSelfPreferencesAsync(HttpContext.GetUserId(), request));
}
