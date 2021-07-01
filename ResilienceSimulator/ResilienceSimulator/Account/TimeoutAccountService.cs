using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilienceSimulator.Account
{
    public class TimeoutAccountService : AccountService, IAccountService
    {
        public TimeoutAccountService(
            ILogger<TimeoutAccountService> logger) : base(logger) { }

        private AsyncTimeoutPolicy ResilientStrategy =>
            Policy.TimeoutAsync(
                timeout: TimeSpan.FromSeconds(1),
                onTimeoutAsync: (_, _, _, e) =>
                {
                    _logger.LogInformation("Timout has occured");
                    return Task.CompletedTask;
                });

        public async Task<long> GetCurrentBalanceAsync(CancellationToken cancellationToken = default)
        {
            return await ResilientStrategy.ExecuteAsync(async (ct) => await GetCurrentBalanceFromBackendAsync(ct), CancellationToken.None);
        }
    }
}
