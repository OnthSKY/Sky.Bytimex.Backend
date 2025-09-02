using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Contract.Requests.Products;
using Sky.Template.Backend.Core.Exceptions;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Sky.Template.Backend.WebAPI.Controllers.Vendor;

[ApiController]
[Route("api/vendor/products")]
[ApiVersion("1.0")]
public class ProductController : VendorBaseController
{
    private readonly IVendorProductService _productService;

    public ProductController(IVendorProductService productService)
    {
        _productService = productService;
    }

    [HttpPost("v{version:apiVersion}/filter")]
    public async Task<IActionResult> GetFilteredProducts([FromBody] ProductFilterRequest request)
        => await HandleServiceResponseAsync(() => _productService.GetFilteredPaginatedProductsAsync(request));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            return await HandleServiceResponseAsync(() => _productService.CreateProductAsync(request));
        }
        catch (MaintenanceModeException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
        }
        catch (MaxProductLimitExceededException ex)
        {
            return Conflict(ex.Message);
        }
        catch (KycRequiredException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
        => await HandleServiceResponseAsync(() => _productService.UpdateProductAsync(id, request));

    [HttpDelete("v{version:apiVersion}/{id}/soft")]
    public async Task<IActionResult> SoftDeleteProduct(Guid id)
        => await HandleServiceResponseAsync(() => _productService.SoftDeleteProductAsync(id));

    [HttpGet("v{version:apiVersion}/{id}/stock-status")]
    public async Task<IActionResult> GetProductStockStatus(Guid id)
        => await HandleServiceResponseAsync(() => _productService.GetProductStockStatusAsync(id));
}
