using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilienceSimulator.Account
{
    public class CacheAccountService : AccountService, IAccountService
    {
        public CacheAccountService(
            ILogger<TimeoutAccountService> logger,
            IMemoryCache memoryCache) : base(logger)
        {
            var memoryCacheProvider = new MemoryCacheProvider(memoryCache);

            ResilientStrategy = Policy.CacheAsync(memoryCacheProvider, TimeSpan.FromMilliseconds(5));
        }

        AsyncCachePolicy ResilientStrategy { get; set; }

        public async Task<long> GetCurrentBalanceAsync(CancellationToken cancellationToken = default)
        {
            return await ResilientStrategy.ExecuteAsync(async (ct) => await GetCurrentBalanceFromBackendAsync(ct), CancellationToken.None);
        }
    }
}
