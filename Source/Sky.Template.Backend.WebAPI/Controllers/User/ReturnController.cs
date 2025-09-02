using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Returns;

namespace Sky.Template.Backend.WebAPI.Controllers.User;

[ApiController]
[Route("api/user/returns")]
[ApiVersion("1.0")]
public class ReturnController : UserBaseController
{
    private readonly IUserReturnService _returnService;

    public ReturnController(IUserReturnService returnService)
    {
        _returnService = returnService;
    }

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> CreateReturn([FromBody] CreateReturnRequest request)
        => await HandleServiceResponseAsync(() => _returnService.CreateReturnAsync(request));

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetMyReturns()
        => await HandleServiceResponseAsync(() => _returnService.GetMyReturnsAsync());

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetReturnById(Guid id)
        => await HandleServiceResponseAsync(() => _returnService.GetReturnByIdAsync(id));

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> CancelReturn(Guid id)
        => await HandleServiceResponseAsync(() => _returnService.CancelReturnAsync(id));
}

