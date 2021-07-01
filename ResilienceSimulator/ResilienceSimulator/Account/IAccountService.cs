using System.Threading;
using System.Threading.Tasks;

namespace ResilienceSimulator.Account
{
    public interface IAccountService
    {
        Task<long> GetCurrentBalanceAsync(CancellationToken cancellationToken = default);
    }
}
