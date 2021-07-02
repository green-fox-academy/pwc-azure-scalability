using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilienceSimulator.Account
{
    public class CircuitBreakerAccountService : AccountService, IAccountService
    {
        public CircuitBreakerAccountService(
            ILogger<RetryAccountService> logger) : base(logger) { }

        private AsyncCircuitBreakerPolicy ResilientStrategy =>
            Policy
            .Handle<AccountException>()
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(10));

        public async Task<long> GetCurrentBalanceAsync(CancellationToken cancellationToken = default)
        {
            return await ResilientStrategy.ExecuteAsync(async (ct) => await GetCurrentBalanceFromBackendAsyncThrow(ct), CancellationToken.None);
        }
    }
}
