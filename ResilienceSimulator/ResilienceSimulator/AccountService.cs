using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilienceSimulator
{
    public interface IAccountService
    {
        Task<long> GetCurrentBalanceAsync(CancellationToken cancellationToken = default);
    }

    public class AccountService : IAccountService
    {
        public Task<long> GetCurrentBalanceAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(10_000L);
        }
    }

    public class BadAccountService : IAccountService
    {
        public async Task<long> GetCurrentBalanceAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(15_000, cancellationToken);

            throw new AccountException();
        }
    }

    public class TimeoutAccountService : IAccountService
    {
        private readonly ILogger<TimeoutAccountService> _logger;

        public TimeoutAccountService(
            ILogger<TimeoutAccountService> logger)
        {
            _logger = logger;
        }

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
            _logger.LogInformation("Start");

            return await ResilientStrategy.ExecuteAsync(async (ct) => await GetCurrentBalanceFromBackend(ct), CancellationToken.None);
        }

        private async Task<long> GetCurrentBalanceFromBackend(CancellationToken cancellationToken = default)
        {
            await Task.Delay(15_000, cancellationToken);

            throw new AccountException();
        }
    }

    public class AccountException : Exception
    {

    }
}
