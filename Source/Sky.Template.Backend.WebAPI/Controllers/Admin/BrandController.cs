using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.BrandRequests;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/brands")]
[ApiVersion("2.0")]
public class BrandController : AdminBaseController
{
    private readonly IAdminBrandService _brandService;

    public BrandController(IAdminBrandService brandService)
    {
        _brandService = brandService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetPaged([FromQuery] BrandFilterRequest request, [FromQuery(Name = "includeInactive")] bool includeInactive = true)
        => await HandleServiceResponseAsync(() => _brandService.GetPagedAsync(request, includeInactive));

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetById(Guid id)
        => await HandleServiceResponseAsync(() => _brandService.GetByIdAsync(id));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> Create([FromBody] CreateBrandRequest request)
        => await HandleServiceResponseAsync(() => _brandService.CreateAsync(request));

    [HttpPut("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBrandRequest request)
    {
        request = request with { Id = id };
        return await HandleServiceResponseAsync(() => _brandService.UpdateAsync(request));
    }

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> SoftDelete(Guid id)
        => await HandleServiceResponseAsync(() => _brandService.SoftDeleteAsync(id));

    [HttpDelete("v{version:apiVersion}/{id}/hard")]
    public async Task<IActionResult> HardDelete(Guid id)
        => await HandleServiceResponseAsync(() => _brandService.HardDeleteAsync(id));
}
