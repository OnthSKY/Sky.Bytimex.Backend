using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Returns;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/returns")]
[ApiVersion("1.0")]
public class ReturnController : AdminBaseController
{
    private readonly IAdminReturnService _returnService;

    public ReturnController(IAdminReturnService returnService)
    {
        _returnService = returnService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetReturns([FromQuery] ReturnFilterRequest request)
        => await HandleServiceResponseAsync(() => _returnService.GetReturnsAsync(request));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> CreateReturn([FromBody] CreateReturnRequest request)
        => await HandleServiceResponseAsync(() => _returnService.CreateReturnAsync(request));

    [HttpPut("v{version:apiVersion}/{id}/status")]
    public async Task<IActionResult> UpdateReturnStatus(Guid id, [FromBody] UpdateReturnStatusRequest request)
        => await HandleServiceResponseAsync(() => _returnService.UpdateReturnStatusAsync(id, request));

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> DeleteReturn(Guid id)
        => await HandleServiceResponseAsync(() => _returnService.DeleteReturnAsync(id));
}

