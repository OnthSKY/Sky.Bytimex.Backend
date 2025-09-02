using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.BuyerAddresses;

namespace Sky.Template.Backend.WebAPI.Controllers.User;

[ApiController]
[Route("api/user/buyer-addresses")]
[ApiVersion("1.0")]
public class BuyerAddressController : UserBaseController
{
    private readonly IUserBuyerAddressService _service;

    public BuyerAddressController(IUserBuyerAddressService service)
    {
        _service = service;
    }

    [HttpGet("v{version:apiVersion}")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetBuyerAddresses([FromQuery] BuyerAddressFilterRequest request)
        => await HandleServiceResponseAsync(() => _service.GetFilteredPaginatedAsync(request));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBuyerAddress(Guid id)
        => await HandleServiceResponseAsync(() => _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> CreateBuyerAddress([FromBody] CreateBuyerAddressRequest request)
        => await HandleServiceResponseAsync(() => _service.CreateAsync(request));

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBuyerAddress(Guid id, [FromBody] UpdateBuyerAddressRequest request)
    {
        request.Id = id;
        return await HandleServiceResponseAsync(() => _service.UpdateAsync(id, request));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBuyerAddress(Guid id)
        => await HandleServiceResponseAsync(() => _service.SoftDeleteAsync(id));
}
