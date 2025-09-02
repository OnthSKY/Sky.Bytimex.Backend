using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Resources;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/resources")]
[ApiVersion("1.0")]
/// <summary>
/// Provides endpoints for managing resources in the admin area.
/// </summary>
public class ResourceController : AdminBaseController
{
    private readonly IAdminResourceService _resourceService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceController"/> class.
    /// </summary>
    /// <param name="resourceService">Service for managing resources.</param>
    public ResourceController(IAdminResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    /// <summary>
    /// Retrieves all resources.
    /// </summary>
    /// <returns>A collection of resources.</returns>
    [HttpGet("v{version:apiVersion}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResourcesAsync()
    {
        return await HandleServiceResponseAsync(() => _resourceService.GetAllAsync());
    }

    /// <summary>
    /// Retrieves a resource by its unique code.
    /// </summary>
    /// <param name="code">The resource code.</param>
    /// <returns>The requested resource.</returns>
    [HttpGet("v{version:apiVersion}/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResourceByCodeAsync(string code)
    {
        return await HandleServiceResponseAsync(() => _resourceService.GetByCodeAsync(code));
    }

    /// <summary>
    /// Creates a new resource.
    /// </summary>
    /// <param name="request">The resource details.</param>
    /// <returns>The result of the creation operation.</returns>
    [HttpPost("v{version:apiVersion}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateResourceAsync([FromBody] CreateResourceRequest request)
    {
        return await HandleServiceResponseAsync(() => _resourceService.CreateAsync(request));
    }

    /// <summary>
    /// Updates an existing resource.
    /// </summary>
    /// <param name="code">The resource code.</param>
    /// <param name="request">Updated resource details.</param>
    /// <returns>The result of the update operation.</returns>
    [HttpPut("v{version:apiVersion}/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateResourceAsync(string code, [FromBody] UpdateResourceRequest request)
    {
        return await HandleServiceResponseAsync(() => _resourceService.UpdateAsync(code, request));
    }

    /// <summary>
    /// Deletes an existing resource.
    /// </summary>
    /// <param name="code">The resource code.</param>
    /// <returns>The result of the delete operation.</returns>
    [HttpDelete("v{version:apiVersion}/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteResourceAsync(string code)
    {
        return await HandleServiceResponseAsync(() => _resourceService.DeleteAsync(code));
    }
}
