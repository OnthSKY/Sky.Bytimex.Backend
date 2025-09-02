using Sky.Template.Backend.Core.Enums;
using System.Threading.Tasks;

namespace Sky.Template.Backend.Application.Services.Payments;

public class Web3PaymentGateway : IPaymentGateway
{
    private readonly IWeb3Service _web3Service;

    public Web3PaymentGateway(IWeb3Service web3Service)
    {
        _web3Service = web3Service;
    }

    public string Method => PaymentType.CRYPTO.ToString();

    public async Task<bool> ConfirmPaymentAsync(PaymentContext context, string transactionHash)
    {
        return await _web3Service.ValidateTransactionHashAsync(transactionHash);
    }
}

