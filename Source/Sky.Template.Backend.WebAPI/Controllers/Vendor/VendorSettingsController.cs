using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Application.Services.System;
using Sky.Template.Backend.Contract.Requests.Settings;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.Extensions;

namespace Sky.Template.Backend.WebAPI.Controllers.Vendor;

[ApiController]
[Route("api/vendor/settings")]
[ApiVersion("1.0")]
public class VendorSettingsController : VendorBaseController
{
    private readonly ISettingsService _settingsService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public VendorSettingsController(ISettingsService settingsService, IHttpContextAccessor httpContextAccessor)
    {
        _settingsService = settingsService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetOverrides()
    {
        var vendorId = _httpContextAccessor.HttpContext.GetUserId();
        return await HandleServiceResponseAsync(() => _settingsService.GetVendorOverridesAsync(vendorId));
    }

    [HttpPut("v{version:apiVersion}/{key}")]
    public async Task<IActionResult> Upsert(string key, [FromBody] UpdateSettingRequest request)
    {
        var vendorId = _httpContextAccessor.HttpContext.GetUserId();
        return await HandleServiceResponseAsync(() => _settingsService.UpsertVendorSettingAsync(vendorId, key, request.Value));
    }
}
