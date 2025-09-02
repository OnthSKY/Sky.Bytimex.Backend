using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.User;
using System.Threading.Tasks;

namespace Sky.Template.Backend.WebAPI.Controllers.User;

[ApiController]
[Route("api/user/payments")]
[ApiVersion("1.0")]
public class PaymentController : UserBaseController
{
    private readonly IUserPaymentService _paymentService;

    public PaymentController(IUserPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("v{version:apiVersion}")]
    public async Task<IActionResult> GetMyPayments()
    {
        var payments = await _paymentService.GetMyPaymentsAsync();
        return Ok(payments);
    }
}

