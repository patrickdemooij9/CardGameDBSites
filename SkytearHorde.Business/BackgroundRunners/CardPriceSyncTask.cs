using Microsoft.Extensions.Logging;
using SkytearHorde.Business.Services;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace SkytearHorde.Business.BackgroundRunners
{
    public class CardPriceSyncTask : RecurringHostedServiceBase
    {
        private readonly CardPriceSyncService _cardPriceSyncService;
        private readonly IRuntimeState _runtimeState;

        public CardPriceSyncTask(ILogger<CardPriceSyncTask> logger, CardPriceSyncService cardPriceSyncService, IRuntimeState runtimeState) : base(logger, TimeSpan.FromHours(2), TimeSpan.FromMinutes(1))
        {
            _cardPriceSyncService = cardPriceSyncService;
            _runtimeState = runtimeState;
        }

        public override Task PerformExecuteAsync(object? state)
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
            {
                return Task.CompletedTask;
            }

            return _cardPriceSyncService.SyncAllSites();
        }
    }

    public class TcgPlayerAccessToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
    }

    public class TcgPlayerResult
    {
        [JsonPropertyName("results")]
        public TcgPlayerPrice[] Results { get; set; }
    }

    public class TcgPlayerPrice
    {
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("lowPrice")]
        public double? LowPrice { get; set; }

        [JsonPropertyName("highPrice")]
        public double? HighPrice { get; set; }

        [JsonPropertyName("marketPrice")]
        public double? MarketPrice { get; set; }

        [JsonPropertyName("subTypeName")]
        public string SubTypeName { get; set; }
    }
}
