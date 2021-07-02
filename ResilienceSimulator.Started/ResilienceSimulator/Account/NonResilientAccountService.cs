using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ResilienceSimulator.Account
{
    public class NonResilientAccountService : AccountService, IAccountService
    {
        public NonResilientAccountService(
            ILogger<NonResilientAccountService> logger) : base(logger) { }

        public async Task<long> GetCurrentBalanceAsync(CancellationToken cancellationToken = default)
        {
            return await GetCurrentBalanceFromBackendAsyncThrow(cancellationToken);
        }
    }
}
