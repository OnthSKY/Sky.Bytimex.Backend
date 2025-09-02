using System.Threading.Tasks;

namespace Sky.Template.Backend.Application.Services.Payments;

public class DummyPaymentGateway : IPaymentGateway
{
    public string Method => "DEFAULT";

    public Task<bool> ConfirmPaymentAsync(PaymentContext context, string transactionHash)
        => Task.FromResult(true);
}

