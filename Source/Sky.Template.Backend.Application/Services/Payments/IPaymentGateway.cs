using System.Threading.Tasks;

namespace Sky.Template.Backend.Application.Services.Payments;

public interface IPaymentGateway
{
    string Method { get; }
    Task<bool> ConfirmPaymentAsync(PaymentContext context, string transactionHash);
}

