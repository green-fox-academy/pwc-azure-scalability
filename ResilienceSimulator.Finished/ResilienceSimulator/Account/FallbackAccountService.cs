using Microsoft.Extensions.Logging;
using Polly;
using Polly.Fallback;
using System.Threading;
using System.Threading.Tasks;

namespace ResilienceSimulator.Account
{
    public class FallbackAccountService : AccountService, IAccountService
    {
        public FallbackAccountService(
            ILogger<TimeoutAccountService> logger) : base(logger) { }

        private AsyncFallbackPolicy<long> ResilientStrategy =>
            Policy<long>
            .Handle<AccountException>()
            .FallbackAsync<long>(
                fallbackAction: async (ct) => await GetCurrentBalanceFallbackAsync(ct),
                onFallbackAsync: (r) =>
                {
                    _logger.LogInformation($"Fallback value: {r.Result.ToString()}");

                    return Task.CompletedTask;
                });

        public async Task<long> GetCurrentBalanceAsync(CancellationToken cancellationToken = default)
        {
            return await ResilientStrategy.ExecuteAsync(async (ct) => await GetCurrentBalanceFromBackendAsyncThrow(ct), CancellationToken.None);
        }

        private async Task<long> GetCurrentBalanceFallbackAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"{nameof(GetCurrentBalanceFallbackAsync)} was called");
            return await Task.FromResult(100L);
        }
    }
}
