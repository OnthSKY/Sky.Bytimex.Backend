using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.User;

namespace Sky.Template.Backend.WebAPI.Controllers.User;

[ApiController]
[Route("api/user/product-categories")]
[ApiVersion("1.0")]
public class ProductCategoryController : UserBaseController
{
    private readonly IUserProductCategoryService _categoryService;
    public ProductCategoryController(IUserProductCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("v{version:apiVersion}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
        => await HandleServiceResponseAsync(() => _categoryService.GetAllCategoriesAsync());

    [HttpGet("v{version:apiVersion}/by-name/{name}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByName(string name)
        => await HandleServiceResponseAsync(() => _categoryService.GetCategoryByNameAsync(name));
}
