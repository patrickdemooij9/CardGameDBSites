using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SkytearHorde.Business.Config;

namespace SkytearHorde.Cache
{
    public class CloudflareCachePurgeService
    {
        private readonly HttpClient _httpClient;
        private readonly CardGameSettingsConfig _config;
        private readonly ILogger<CloudflareCachePurgeService> _logger;

        public CloudflareCachePurgeService(HttpClient httpClient, IOptions<CardGameSettingsConfig> config,
            ILogger<CloudflareCachePurgeService> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;
        }

        public void PurgeUrls(IEnumerable<string> urls)
        {
            if (string.IsNullOrWhiteSpace(_config.CloudflareApiToken) || string.IsNullOrWhiteSpace(_config.CloudflareZoneId))
            {
                return;
            }

            var validUrls = urls.Where(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (validUrls.Length == 0)
            {
                return;
            }

            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.cloudflare.com/client/v4/zones/{_config.CloudflareZoneId}/purge_cache");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.CloudflareApiToken);

            var payload = JsonSerializer.Serialize(new
            {
                files = validUrls
            });
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Cloudflare homepage cache purge failed with status code {StatusCode}.", response.StatusCode);
            }
        }
    }
}
