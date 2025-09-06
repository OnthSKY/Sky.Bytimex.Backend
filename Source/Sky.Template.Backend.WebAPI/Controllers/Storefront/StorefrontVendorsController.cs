using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Storefront;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.WebAPI.Controllers.Base;

namespace Sky.Template.Backend.WebAPI.Controllers.Storefront;

[ApiController]
[Route("api/storefront/v{version:apiVersion}/vendors")]
[ApiVersion("1.0")]
public class StorefrontVendorsController : CustomBaseController
{
    private readonly IStorefrontVendorService _service;

    public StorefrontVendorsController(IStorefrontVendorService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    [HttpGet]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetVendors([FromQuery] GridRequest request)
        => await HandleServiceResponseAsync(() => _service.GetVendorsAsync(request));

    [AllowAnonymous]
    [HttpGet("detail")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetVendorDetail([FromQuery] string? slug, [FromQuery] Guid? id)
    {
        if (string.IsNullOrWhiteSpace(slug) && !id.HasValue)
            return BadRequest("SlugOrIdRequired");
        if (!string.IsNullOrWhiteSpace(slug) && !Regex.IsMatch(slug, "^[a-z0-9-]+$"))
            return BadRequest("InvalidSlug");
        return await HandleServiceResponseAsync(() => _service.GetVendorDetailAsync(slug, id));
    }
}
