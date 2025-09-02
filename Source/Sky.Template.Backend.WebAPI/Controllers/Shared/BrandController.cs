using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.BrandRequests;
using Sky.Template.Backend.WebAPI.Controllers.Base;

namespace Sky.Template.Backend.WebAPI.Controllers.Shared;

[ApiController]
[Route("api/brands")]
[ApiVersion("1.0")]
public class BrandController : CustomBaseController
{
    private readonly IBrandService _brandService;

    public BrandController(IBrandService brandService)
    {
        _brandService = brandService;
    }
    [AllowAnonymous]
    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetPaged([FromQuery] BrandFilterRequest request)
        => await HandleServiceResponseAsync(() => _brandService.GetPagedAsync(request));
    [AllowAnonymous]
    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetById(Guid id)
        => await HandleServiceResponseAsync(() => _brandService.GetByIdAsync(id));
}
