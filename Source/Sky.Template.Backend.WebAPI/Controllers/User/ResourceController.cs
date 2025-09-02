using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.User;

namespace Sky.Template.Backend.WebAPI.Controllers.User;

[ApiController]
[Route("api/user/resources")]
[ApiVersion("1.0")]
public class ResourceController : UserBaseController
{
    private readonly IUserResourceService _resourceService;

    public ResourceController(IUserResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetMyResources()
    {
        return await HandleServiceResponseAsync(() => _resourceService.GetCurrentUserResourcesAsync());
    }
}
