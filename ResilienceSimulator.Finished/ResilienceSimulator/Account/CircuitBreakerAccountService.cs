using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilienceSimulator.Account
{
    public class CircuitBreakerAccountService : AccountService, IAccountService
    {
        public CircuitBreakerAccountService(
            ILogger<RetryAccountService> logger) : base(logger)
        {
            ResilientStrategy = Policy.WrapAsync(RetryResilientStrategy, CircuitBreakerResilientStrategy);
        }

        private AsyncCircuitBreakerPolicy CircuitBreakerResilientStrategy =>
            Policy
            .Handle<AccountException>()
            .Or<TimeoutRejectedException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 2,
                durationOfBreak: TimeSpan.FromMilliseconds(1000),
                onBreak: (e, ts) => _logger.LogInformation($"Closed->Open {e.GetType().Name} - {ts.ToString()}"),
                onReset: () => _logger.LogInformation("Open->Closed"),
                onHalfOpen: () => _logger.LogInformation("Half open"));

        private AsyncRetryPolicy RetryResilientStrategy =>
            Policy
            .Handle<AccountException>()
            .Or<BrokenCircuitException>()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                retryCount: 10,
                sleepDurationProvider: (tn) => TimeSpan.FromMilliseconds(200),
                onRetryAsync: (e, ts) =>
                {
                    _logger.LogInformation($"Retry: {e.GetType().Name} - {ts.ToString()}");

                    return Task.CompletedTask;
                });

        AsyncPolicyWrap ResilientStrategy { get; set; }

        public async Task<long> GetCurrentBalanceAsync(CancellationToken cancellationToken = default)
        {
            return await ResilientStrategy.ExecuteAsync(async (ct) => await GetCurrentBalanceFromBackendAsyncThrow(ct), CancellationToken.None);
        }
    }
}
