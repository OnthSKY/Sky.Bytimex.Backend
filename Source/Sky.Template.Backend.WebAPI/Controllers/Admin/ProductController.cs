using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Products;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/products")]
[ApiVersion("1.0")]
public class ProductController : AdminBaseController
{
    private readonly IAdminProductService _productService;

    public ProductController(IAdminProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetAll([FromQuery] ProductFilterRequest request)
        => await HandleServiceResponseAsync(() => _productService.GetAllProductsAsync(request));

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetById(Guid id)
        => await HandleServiceResponseAsync(() => _productService.GetProductByIdAsync(id));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        => await HandleServiceResponseAsync(() => _productService.CreateProductAsync(request));

    [HttpPut("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
        => await HandleServiceResponseAsync(() => _productService.UpdateProductAsync(id, request));

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> HardDeleteProduct(Guid id)
        => await HandleServiceResponseAsync(() => _productService.HardDeleteProductAsync(id));

    [HttpPost("v{version:apiVersion}/images")]
    public async Task<IActionResult> AddImage([FromBody] AddImageRequest request)
        => await HandleServiceResponseAsync(() => _productService.AddProductImageAsync(request));

    [HttpPost("v{version:apiVersion}/attributes")]
    public async Task<IActionResult> UpsertAttribute([FromBody] UpsertAttributeRequest request)
        => await HandleServiceResponseAsync(() => _productService.UpsertProductAttributeAsync(request));

    [HttpPost("v{version:apiVersion}/variants")]
    public async Task<IActionResult> CreateVariant([FromBody] CreateVariantRequest request)
        => await HandleServiceResponseAsync(() => _productService.CreateVariantAsync(request));

    [HttpPost("v{version:apiVersion}/variants/attributes")]
    public async Task<IActionResult> SetVariantAttribute([FromBody] SetVariantAttributeRequest request)
        => await HandleServiceResponseAsync(() => _productService.SetVariantAttributeAsync(request));
}
