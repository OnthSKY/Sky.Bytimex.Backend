using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Auth;
using Sky.Template.Backend.WebAPI.Controllers.Base;

namespace Sky.Template.Backend.WebAPI.Controllers.User;

[ApiController]
[Route("api/auth")]
public class UserAuthController : CustomBaseController
{
    private readonly IUserAuthService _authService;

    public UserAuthController(IUserAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register/user")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        => await HandleServiceResponseAsync(() => _authService.RegisterAsync(request));
}
