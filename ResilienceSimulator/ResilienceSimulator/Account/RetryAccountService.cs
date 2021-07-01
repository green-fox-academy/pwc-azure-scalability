using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilienceSimulator.Account
{
    public class RetryAccountService : IAccountService
    {
        private readonly ILogger<RetryAccountService> _logger;

        public RetryAccountService(
            ILogger<RetryAccountService> logger)
        {
            _logger = logger;
        }

        private AsyncRetryPolicy ResilientStrategy =>
            Policy
            .Handle<AccountException>()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                retryCount: 2,
                sleepDurationProvider: (tn) => TimeSpan.FromSeconds(1));

        public async Task<long> GetCurrentBalanceAsync(CancellationToken cancellationToken = default)
        {
            return await ResilientStrategy.ExecuteAsync(async (ct) => await GetCurrentBalanceFromBackend(ct), CancellationToken.None);
        }

        private async Task<long> GetCurrentBalanceFromBackend(CancellationToken cancellationToken = default)
        {
            await Task.Delay(15_000, cancellationToken);

            throw new AccountException();
        }
    }
}
