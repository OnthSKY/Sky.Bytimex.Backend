using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Buyers;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/buyers")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class BuyerController : AdminBaseController
{
    private readonly IAdminBuyerService _buyerService;

    public BuyerController(IAdminBuyerService buyerService)
    {
        _buyerService = buyerService;
    }

    [HttpGet("v{version:apiVersion}")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetAllBuyers()
        => await HandleServiceResponseAsync(() => _buyerService.GetAllBuyersAsync());

    [HttpGet("v{version:apiVersion}")]
    [MapToApiVersion("2")]
    public async Task<IActionResult> GetFilteredPaginatedBuyers([FromQuery] BuyerFilterRequest request)
        => await HandleServiceResponseAsync(() => _buyerService.GetFilteredPaginatedBuyersAsync(request));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBuyerById(Guid id)
        => await HandleServiceResponseAsync(() => _buyerService.GetBuyerByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> CreateBuyer([FromBody] CreateBuyerRequest request)
        => await HandleServiceResponseAsync(() => _buyerService.CreateBuyerAsync(request));

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBuyer(Guid id, [FromBody] UpdateBuyerRequest request)
        => await HandleServiceResponseAsync(() => _buyerService.UpdateBuyerAsync(id, request));

    [HttpDelete("{id}/soft")]
    public async Task<IActionResult> SoftDeleteBuyer(Guid id)
        => await HandleServiceResponseAsync(() => _buyerService.SoftDeleteBuyerAsync(id));

    [HttpDelete("{id}")]
    public async Task<IActionResult> HardDeleteBuyer(Guid id)
        => await HandleServiceResponseAsync(() => _buyerService.HardDeleteBuyerAsync(id));

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetBuyerByEmail(string email)
        => await HandleServiceResponseAsync(() => _buyerService.GetBuyerByEmailAsync(email));

    [HttpGet("phone/{phone}")]
    public async Task<IActionResult> GetBuyerByPhone(string phone)
        => await HandleServiceResponseAsync(() => _buyerService.GetBuyerByPhoneAsync(phone));
}
