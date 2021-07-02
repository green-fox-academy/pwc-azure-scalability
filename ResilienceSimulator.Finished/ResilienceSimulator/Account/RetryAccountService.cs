using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilienceSimulator.Account
{
    public class RetryAccountService : AccountService, IAccountService
    {
        public RetryAccountService(
            ILogger<RetryAccountService> logger) : base(logger) { }

        private AsyncRetryPolicy ResilientStrategy =>
            Policy
            .Handle<AccountException>()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: (tn) => TimeSpan.FromSeconds(Math.Pow(2, tn)),
                onRetryAsync: (e, ts) =>
                {
                    _logger.LogInformation($"Retry: {e.GetType().Name} - {ts.ToString()}");

                    return Task.CompletedTask;
                });

        public async Task<long> GetCurrentBalanceAsync(CancellationToken cancellationToken = default)
        {
            return await ResilientStrategy.ExecuteAsync(async (ct) => await GetCurrentBalanceFromBackendAsyncThrow(ct), CancellationToken.None);
        }
    }
}
