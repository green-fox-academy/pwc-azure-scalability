using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ResilienceSimulator.Account
{
    public class BulkheadAccountService : AccountService, IAccountService
    {
        public BulkheadAccountService(ILogger<BulkheadAccountService> logger) : base(logger)
        {

        }

        public async Task<long> GetCurrentBalanceAsync(CancellationToken cancellationToken = default)
        {
            var resilientStrategy =
                Policy.BulkheadAsync(
                    maxParallelization: 2,
                    maxQueuingActions: 2,
                    onBulkheadRejectedAsync: (c) =>
                    {
                        _logger.LogInformation($"Bulkhead rejected");
                        return Task.CompletedTask;
                    });

            var parallelTasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                _logger.LogInformation($"Scheduling, Available: {resilientStrategy.BulkheadAvailableCount}, QueueAvailable: {resilientStrategy.QueueAvailableCount}");
                var task = resilientStrategy.ExecuteAsync(async () =>
                {
                    await DoSomethingAsync();
                });
                parallelTasks.Add(task);
                await Task.Delay(50);
            }

            await Task.WhenAll(parallelTasks);

            return 1000;
        }

        private async Task DoSomethingAsync()
        {
            await Task.Delay(1000);
        }
    }
}
