using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ResilienceSimulator.Account
{

    public class AccountService
    {
        private int numberOfCalls;
        protected readonly ILogger<AccountService> _logger;

        public AccountService(
            ILogger<AccountService> logger)
        {
            _logger = logger;
        }

        protected async Task<long> GetCurrentBalanceFromBackendAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"{nameof(GetCurrentBalanceFromBackendAsync)} called {numberOfCalls++} times.");

            await Task.Delay(5_000, cancellationToken);

            return await Task.FromResult(200);
        }

        protected async Task<long> GetCurrentBalanceFromBackendAsyncThrow(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"{nameof(GetCurrentBalanceFromBackendAsyncThrow)} called {numberOfCalls++} times.");

            await Task.Delay(5_000, cancellationToken);

            throw new AccountException();
        }
    }
}
