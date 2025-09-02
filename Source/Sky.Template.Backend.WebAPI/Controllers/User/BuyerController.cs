using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Buyers;

namespace Sky.Template.Backend.WebAPI.Controllers.User;

[ApiController]
[Route("api/user/buyers")]
[ApiVersion("1.0")]
public class BuyerController : UserBaseController
{
    private readonly IUserBuyerService _buyerService;

    public BuyerController(IUserBuyerService buyerService)
    {
        _buyerService = buyerService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetMyBuyers()
        => await HandleServiceResponseAsync(() => _buyerService.GetMyBuyersAsync());

    [HttpPost("v{version:apiVersion}")]
    public async Task<IActionResult> CreateBuyer([FromBody] CreateBuyerRequest request)
        => await HandleServiceResponseAsync(() => _buyerService.CreateBuyerAsync(request));

    [HttpPut("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> UpdateBuyer(Guid id, [FromBody] UpdateBuyerRequest request)
        => await HandleServiceResponseAsync(() => _buyerService.UpdateBuyerAsync(id, request));

    [HttpDelete("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> DeleteBuyer(Guid id)
        => await HandleServiceResponseAsync(() => _buyerService.DeleteBuyerAsync(id));

    [HttpGet("v{version:apiVersion}/{id}")]
    public async Task<IActionResult> GetBuyerById(Guid id)
        => await HandleServiceResponseAsync(() => _buyerService.GetBuyerByIdAsync(id));
}
