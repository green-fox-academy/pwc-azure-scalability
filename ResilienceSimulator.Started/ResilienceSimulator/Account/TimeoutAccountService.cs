using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ResilienceSimulator.Account
{
    internal class TimeoutAccountService : AccountService, IAccountService
    {
        public TimeoutAccountService(ILogger<TimeoutAccountService> logger) : base(logger)
        {

        }

        private AsyncTimeoutPolicy ResilientStrategy =>
            Policy.TimeoutAsync(
                timeout: TimeSpan.FromSeconds(1),
                timeoutStrategy: TimeoutStrategy.Pessimistic,
                onTimeoutAsync: async (c, ts, task, e) =>
                {
                    Console.WriteLine(task.Status);

                    await Task.Delay(100);

                    Console.WriteLine(task.Status);

                    _logger.LogError(e, $"Timout has occured Exception: {e.GetType().Name}, Timeout: {ts.ToString()}");
                });

        public async Task<long> GetCurrentBalanceAsync(CancellationToken cancellationToken = default)
        {
            //var externalCts = new CancellationTokenSource();
            //externalCts.CancelAfter(TimeSpan.FromSeconds(1));
            return await ResilientStrategy.ExecuteAsync(async (ct) => await GetCurrentBalanceFromBackendAsyncThrow(ct), cancellationToken);
        }
    }
}
