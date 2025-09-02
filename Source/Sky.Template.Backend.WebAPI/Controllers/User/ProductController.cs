using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Products;

namespace Sky.Template.Backend.WebAPI.Controllers.User;

[ApiController]
[Route("api/user/products")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class ProductController : UserBaseController
{
    private readonly IUserProductService _productService;

    public ProductController(IUserProductService productService)
    {
        _productService = productService;
    }

    [AllowAnonymous]
    [HttpGet("v{version:apiVersion}")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetAllProducts()
        => await HandleServiceResponseAsync(() => _productService.GetAllProductsAsync());

    [HttpGet("v{version:apiVersion}")]
    [AllowAnonymous]
    [MapToApiVersion("2")]
    public async Task<IActionResult> GetFilteredPaginatedProducts([FromQuery] ProductFilterRequest request)
        => await HandleServiceResponseAsync(() => _productService.GetFilteredPaginatedProductsAsync(request));

    [HttpGet("v{version:apiVersion}/{productId}/variants")]
    [AllowAnonymous]
    [MapToApiVersion("2")]
    public async Task<IActionResult> GetProductVariants(Guid productId, [FromQuery] ProductVariantFilterRequest request)
        => await HandleServiceResponseAsync(() => _productService.GetProductVariantsAsync(productId, request));


    [HttpGet("v{version:apiVersion}/{id}")]
    [AllowAnonymous]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetProductById(Guid id)
        => await HandleServiceResponseAsync(() => _productService.GetProductByIdAsync(id));

    [HttpGet("v{version:apiVersion}/{id}/stock-status")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetProductStockStatus(Guid id)
        => await HandleServiceResponseAsync(() => _productService.GetProductStockStatusAsync(id));
}
