using Haihv.Elis.Tools.Maui.Services;
using Haihv.Elis.Tools.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;

namespace Haihv.Elis.Tools.Maui.Extensions;

public static class HybridCacheWithFileCacheExtensions
{
    public static void AddHybridCacheWithFileCache(this IServiceCollection services, string pathToCache)
    {
        // Register the distributed cache using the file service
        services.AddSingleton<IDistributedCache>(sp =>
            new FileDistributedCache(
                sp.GetRequiredService<IFileService>(), pathToCache));
        services.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = 1024 * 1024;
            options.MaximumKeyLength = 1024;
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(60),
                LocalCacheExpiration = TimeSpan.FromDays(1)
            };
        });
    }
}