using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Contract.Requests.BrandRequests;

namespace Sky.Template.Backend.WebAPI.Controllers.Vendor;

[ApiController]
[Route("api/vendor/brands")]
[ApiVersion("2.0")]
public class BrandController : VendorBaseController
{
    private readonly IVendorBrandService _brandService;

    public BrandController(IVendorBrandService brandService)
    {
        _brandService = brandService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetPaged([FromQuery] BrandFilterRequest request)
        => await HandleServiceResponseAsync(() => _brandService.GetPagedAsync(request));

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetById(Guid id)
        => await HandleServiceResponseAsync(() => _brandService.GetByIdAsync(id));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> Create([FromBody] CreateBrandRequest request)
        => await HandleServiceResponseAsync(() => _brandService.CreateAsync(request));
}
