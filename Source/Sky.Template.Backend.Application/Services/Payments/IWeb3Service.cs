using System.Threading.Tasks;

namespace Sky.Template.Backend.Application.Services.Payments;

public interface IWeb3Service
{
    Task<bool> ValidateTransactionHashAsync(string transactionHash);
}

public class Web3Service : IWeb3Service
{
    public Task<bool> ValidateTransactionHashAsync(string transactionHash)
        => Task.FromResult(true);
}

