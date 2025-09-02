using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Vendor;
using Microsoft.AspNetCore.Authorization;
using Sky.Template.Backend.Contract.Requests.Auth;
using Sky.Template.Backend.WebAPI.Controllers.Base;

namespace Sky.Template.Backend.WebAPI.Controllers.Vendor;

[ApiController]
public class VendorAuthController : CustomBaseController
{
    private readonly IVendorAuthService _authService;

    public VendorAuthController(IVendorAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("api/auth/register/vendor")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterVendor([FromBody] RegisterVendorRequest request)
        => await HandleServiceResponseAsync(() => _authService.RegisterVendorAsync(request));
}
