using System.Threading;
using System.Threading.Tasks;

namespace ResilienceSimulator.Account
{

    public class AccountService : IAccountService
    {
        public Task<long> GetCurrentBalanceAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(10_000L);
        }
    }
}
