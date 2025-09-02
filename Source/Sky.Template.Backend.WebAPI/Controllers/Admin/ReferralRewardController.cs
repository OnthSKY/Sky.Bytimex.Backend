using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.ReferralRewards;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/referral-rewards")]
[ApiVersion("1.0")]
public class ReferralRewardController : AdminBaseController
{
    private readonly IAdminReferralRewardService _service;

    public ReferralRewardController(IAdminReferralRewardService service)
    {
        _service = service;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetList([FromQuery] Guid? referrerUserId, [FromQuery] Guid? referredUserId, [FromQuery] string? status)
        => await HandleServiceResponseAsync(() => _service.GetListAsync(referrerUserId, referredUserId, status));

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetById(Guid id)
        => await HandleServiceResponseAsync(() => _service.GetByIdAsync(id));

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> Create([FromBody] CreateReferralRewardRequest request)
        => await HandleServiceResponseAsync(() => _service.CreateAsync(request));

    [HttpPut("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReferralRewardRequest request)
        => await HandleServiceResponseAsync(() => _service.UpdateAsync(id, request));

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> Delete(Guid id)
        => await HandleServiceResponseAsync(() => _service.DeleteAsync(id));
}
