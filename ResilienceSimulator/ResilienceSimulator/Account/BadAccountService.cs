using System.Threading;
using System.Threading.Tasks;

namespace ResilienceSimulator.Account
{
    public class BadAccountService : IAccountService
    {
        public async Task<long> GetCurrentBalanceAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(15_000, cancellationToken);

            throw new AccountException();
        }
    }
}
