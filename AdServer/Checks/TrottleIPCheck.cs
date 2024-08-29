using AdServer.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace AdServer.Checks
{
    public class TrottleIPCheck : IRequestCheck
    {
        private readonly IMemoryCache _memoryCache;

        public TrottleIPCheck(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public bool Check(HttpContext context, TrackingSource source)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(ip)) return true;

            var hasEntry = _memoryCache.Get($"IPTrottle-{ip}-{source}") != null;
            _memoryCache.Set($"IPTrottle-{ip}", ip, DateTimeOffset.UtcNow.AddSeconds(30));
            return !hasEntry;
        }
    }
}
