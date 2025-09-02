using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.System;
using Sky.Template.Backend.Contract.Requests.Settings;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/settings")]
[ApiVersion("1.0")]
public class AdminSettingsController : AdminBaseController
{
    private readonly ISettingsService _settingsService;

    public AdminSettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetAll()
        => await HandleServiceResponseAsync(() => _settingsService.GetAllGlobalSettingsAsync());

    [HttpPut("v{version:apiVersion}/{key}")]
    public async Task<IActionResult> Update(string key, [FromBody] UpdateSettingRequest request)
        => await HandleServiceResponseAsync(() => _settingsService.UpdateGlobalSettingAsync(key, request.Value));
}
