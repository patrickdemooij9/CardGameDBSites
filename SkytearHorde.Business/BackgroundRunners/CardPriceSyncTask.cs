using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkytearHorde.Business.Config;
using SkytearHorde.Business.Integrations.TcgPlayer;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Models.Business;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Extensions;

namespace SkytearHorde.Business.BackgroundRunners
{
    public class CardPriceSyncTask : RecurringHostedServiceBase
    {
        private readonly ILogger<CardPriceSyncTask> _logger;
        private readonly HttpClient _httpClient;
        private readonly ISiteService _siteService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly CardService _cardService;
        private readonly SettingsService _settingsService;
        private readonly CardPriceRepository _cardPriceRepository;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IUmbracoIndexingHandler _umbracoIndexingHandler;
        private readonly IContentService _contentService;
        private readonly CardGameSettingsConfig _cardGameSettingsConfig;
        private readonly IServiceProvider _serviceProvider;

        public CardPriceSyncTask(ILogger<CardPriceSyncTask> logger, HttpClient httpClient, ISiteService siteService, ISiteAccessor siteAccessor, CardService cardService, SettingsService settingsService, CardPriceRepository cardPriceRepository, IUmbracoContextFactory umbracoContextFactory, IUmbracoIndexingHandler umbracoIndexingHandler, IContentService contentService, IOptions<CardGameSettingsConfig> cardGameSettingsConfigOption, IServiceProvider serviceProvider) : base(logger, TimeSpan.FromHours(2), TimeSpan.FromMinutes(1))
        {
            _logger = logger;
            _httpClient = httpClient;
            _siteService = siteService;
            _siteAccessor = siteAccessor;
            _cardService = cardService;
            _settingsService = settingsService;
            _cardPriceRepository = cardPriceRepository;
            _umbracoContextFactory = umbracoContextFactory;
            _umbracoIndexingHandler = umbracoIndexingHandler;
            _contentService = contentService;
            _cardGameSettingsConfig = cardGameSettingsConfigOption.Value;
            _serviceProvider = serviceProvider;
        }

        public async override Task PerformExecuteAsync(object? state)
        {
            _logger.LogInformation("Starting card price sync");
            try
            {
                var accessToken = await (new TcgPlayerAccessTokenGetter().Get(_cardGameSettingsConfig, _httpClient));

                using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
                foreach (var siteId in _siteService.GetAllSites())
                {
                    _siteAccessor.SetSiteId(siteId);

                    if (!_settingsService.GetSiteSettings().AllowPricing)
                        continue;

                    using var scope = _serviceProvider.CreateScope();
                    var collectionService = scope.ServiceProvider.GetService<CollectionService>()!;

                    var variantTypes = collectionService.GetVariantTypes().Where(it => !it.ChildOfBase && !it.ChildOf.HasValue).Select(it => it.Id).ToArray();

                    var cards = _cardService.GetAll(true).Where(it => it.VariantId != 0 && (it.VariantTypeId is null || variantTypes.Contains(it.VariantTypeId.Value))).ToArray();
                    var cardsToSync = new Dictionary<int, Card>();
                    foreach (var card in cards)
                    {
                        var externalIdString = card.GetMultipleCardAttributeValue("TcgPlayerId")?.FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(externalIdString) && int.TryParse(externalIdString, out var externalId))
                        {
                            if (cardsToSync.ContainsKey(externalId))
                            {
                                _logger.LogError($"Found duplicate external id: {externalId}");
                                continue;
                            }

                            cardsToSync.Add(externalId, card);
                        }
                    }

                    _logger.LogInformation("Found {cards} to sync!", cardsToSync.Count);

                    foreach (var group in cardsToSync.InGroupsOf(50))
                    {
                        var ids = group.Select(it => it.Key.ToString()).ToArray();
                        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.tcgplayer.com/pricing/product/{string.Join(",", ids)}");
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                        var priceData = await _httpClient.SendAsync(request);
                        if (priceData.IsSuccessStatusCode)
                        {
                            var content = await priceData.Content.ReadFromJsonAsync<TcgPlayerResult>()!;

                            _cardPriceRepository.InsertPrices(content.Results
                                .Where(it => cardsToSync.ContainsKey(it.ProductId) && it.SubTypeName == "Normal" && it.MarketPrice.HasValue && it.LowPrice.HasValue && it.HighPrice.HasValue)
                                .GroupBy(it => cardsToSync[it.ProductId].BaseId)
                                .Select(it => new CardPriceGroup()
                                {
                                    SourceId = 0,
                                    CardId = it.Key,
                                    Prices = it.Select(c => new CardPrice
                                    {
                                        VariantId = cardsToSync[c.ProductId].VariantId,
                                        MainPrice = c.MarketPrice!.Value,
                                        LowestPrice = c.LowPrice!.Value,
                                        HighestPrice = c.HighPrice!.Value,
                                        DateUtc = DateTime.UtcNow.Date
                                    }).ToList()
                                }).ToArray());
                        }
                    }

                    foreach (var umbracoCard in _contentService.GetByIds(cardsToSync.Values.Select(c => c.BaseId).Distinct()))
                    {
                        _umbracoIndexingHandler.ReIndexForContent(umbracoCard, true);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong");
            }
            _logger.LogInformation("Finished card price sync");
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
