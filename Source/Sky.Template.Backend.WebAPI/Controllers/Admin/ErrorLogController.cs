using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.ErrorLogs;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/error-logs")]
[ApiVersion("1.0")]
public class ErrorLogController : AdminBaseController
{
    private readonly IAdminErrorLogService _errorLogService;

    public ErrorLogController(IAdminErrorLogService errorLogService)
    {
        _errorLogService = errorLogService;
    }

    [HttpPost("v{version:apiVersion}")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> Create([FromBody] CreateErrorLogRequest request)
    {
        await _errorLogService.LogErrorAsync(request);
        return Created(string.Empty, null);
    }

    [HttpGet("v{version:apiVersion}")]
    [MapToApiVersion("1")]
   public async Task<IActionResult> GetLogs([FromQuery] GridRequest request)
    {
        return await HandleServiceResponseAsync(() => _errorLogService.GetAllAsync(request));
    }

    [HttpGet("v{version:apiVersion}/{id}")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetLogById(Guid id)
    {
        return await HandleServiceResponseAsync(() => _errorLogService.GetByIdAsync(id));
    }
}
