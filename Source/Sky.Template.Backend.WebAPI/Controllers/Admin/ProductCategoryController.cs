using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Products;
using Sky.Template.Backend.WebAPI.Controllers.Base;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/product-categories")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class ProductCategoryController : AdminBaseController
{
    private readonly IAdminProductCategoryService _categoryService;
    public ProductCategoryController(IAdminProductCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetAll()
        => await HandleServiceResponseAsync(() => _categoryService.GetAllCategoriesAsync());

    [HttpGet("v{version:apiVersion}/filter")]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetFiltered([FromQuery] ProductCategoryFilterRequest request)
        => await HandleServiceResponseAsync(() => _categoryService.GetFilteredPaginatedCategoriesAsync(request));

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetById(Guid id)
        => await HandleServiceResponseAsync(() => _categoryService.GetCategoryByIdAsync(id));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> Create([FromBody] CreateProductCategoryRequest request)
        => await HandleServiceResponseAsync(() => _categoryService.CreateCategoryAsync(request));

    [HttpPut("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCategoryRequest request)
        => await HandleServiceResponseAsync(() => _categoryService.UpdateCategoryAsync(id, request));

    [HttpDelete("v{version:apiVersion}/{id}/soft")]
    public async Task<IActionResult> SoftDelete(Guid id)
        => await HandleServiceResponseAsync(() => _categoryService.SoftDeleteCategoryAsync(id));

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> HardDelete(Guid id)
        => await HandleServiceResponseAsync(() => _categoryService.HardDeleteCategoryAsync(id));
}
