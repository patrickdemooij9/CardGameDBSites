using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkytearHorde.Business.BackgroundRunners;
using SkytearHorde.Business.Config;
using SkytearHorde.Business.Integrations.TcgPlayer;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Models.Business;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Services
{
    public class CardPriceSyncService
    {
        public const string TcgPlayerIdAttribute = "TcgPlayerId";

        private readonly ILogger<CardPriceSyncService> _logger;
        private readonly HttpClient _httpClient;
        private readonly ISiteService _siteService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly CardService _cardService;
        private readonly SettingsService _settingsService;
        private readonly CardPriceRepository _cardPriceRepository;
        private readonly SetPriceRepository _setPriceRepository;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IUmbracoIndexingHandler _umbracoIndexingHandler;
        private readonly IContentService _contentService;
        private readonly CardGameSettingsConfig _cardGameSettingsConfig;
        private readonly IServiceProvider _serviceProvider;

        private readonly SemaphoreSlim _syncLock = new(1, 1);
        private CardPriceSyncStatus _lastRun = new();

        public CardPriceSyncService(ILogger<CardPriceSyncService> logger, HttpClient httpClient, ISiteService siteService, ISiteAccessor siteAccessor, CardService cardService, SettingsService settingsService, CardPriceRepository cardPriceRepository, SetPriceRepository setPriceRepository, IUmbracoContextFactory umbracoContextFactory, IUmbracoIndexingHandler umbracoIndexingHandler, IContentService contentService, IOptions<CardGameSettingsConfig> cardGameSettingsConfigOption, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _httpClient = httpClient;
            _siteService = siteService;
            _siteAccessor = siteAccessor;
            _cardService = cardService;
            _settingsService = settingsService;
            _cardPriceRepository = cardPriceRepository;
            _setPriceRepository = setPriceRepository;
            _umbracoContextFactory = umbracoContextFactory;
            _umbracoIndexingHandler = umbracoIndexingHandler;
            _contentService = contentService;
            _cardGameSettingsConfig = cardGameSettingsConfigOption.Value;
            _serviceProvider = serviceProvider;
        }

        public CardPriceSyncStatus GetStatus()
        {
            var lastRun = _lastRun;
            return new CardPriceSyncStatus
            {
                IsRunning = _syncLock.CurrentCount == 0,
                StartedUtc = lastRun.StartedUtc,
                FinishedUtc = lastRun.FinishedUtc,
                SyncedCards = lastRun.SyncedCards,
                Error = lastRun.Error
            };
        }

        public static string? GetTcgPlayerId(Card card)
        {
            return card.GetMultipleCardAttributeValue(TcgPlayerIdAttribute)?.FirstOrDefault()?.Trim();
        }

        public Card[] GetPriceableCards()
        {
            using var scope = _serviceProvider.CreateScope();
            var collectionService = scope.ServiceProvider.GetRequiredService<CollectionService>();
            var variantTypes = collectionService.GetVariantTypes()
                .Where(it => !it.ChildOfBase && !it.ChildOf.HasValue)
                .Select(it => it.Id)
                .ToHashSet();

            return _cardService.GetAll(true)
                .Where(it => it.VariantId != 0 && (it.VariantTypeId is null || variantTypes.Contains(it.VariantTypeId.Value)))
                .ToArray();
        }

        public async Task SyncAllSites()
        {
            if (!await _syncLock.WaitAsync(0))
            {
                _logger.LogInformation("Skipping card price sync, a sync is already running");
                return;
            }

            try
            {
                await SyncLocked(_siteService.GetAllSites().ToArray(), BeginRun());
            }
            finally
            {
                _syncLock.Release();
            }
        }

        public bool TryStartSiteSync(int siteId)
        {
            if (!_syncLock.Wait(0)) return false;

            var run = BeginRun();

            // Don't let the request's HttpContext flow into a sync that outlives the request.
            using (ExecutionContext.SuppressFlow())
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SyncLocked([siteId], run);
                    }
                    finally
                    {
                        _syncLock.Release();
                    }
                });
            }
            return true;
        }

        private CardPriceSyncStatus BeginRun()
        {
            var run = new CardPriceSyncStatus { StartedUtc = DateTime.UtcNow };
            _lastRun = run;
            return run;
        }

        private async Task SyncLocked(int[] siteIds, CardPriceSyncStatus run)
        {
            _logger.LogInformation("Starting card price sync");
            try
            {
                var accessToken = await new TcgPlayerAccessTokenGetter().Get(_cardGameSettingsConfig, _httpClient);

                using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
                foreach (var siteId in siteIds)
                {
                    _siteAccessor.SetSiteId(siteId);

                    if (!_settingsService.GetSiteSettings().AllowPricing)
                        continue;

                    run.SyncedCards += await SyncSite(accessToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong");
                run.Error = ex.Message;
            }
            run.FinishedUtc = DateTime.UtcNow;
            _logger.LogInformation("Finished card price sync");
        }

        private async Task<int> SyncSite(string accessToken)
        {
            var cardsToSync = new Dictionary<int, Card>();
            foreach (var card in GetPriceableCards())
            {
                if (int.TryParse(GetTcgPlayerId(card), out var externalId))
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

            SyncSetPrices();
            return cardsToSync.Count;
        }

        private void SyncSetPrices()
        {
            _logger.LogInformation("Starting set price sync");

            var sets = _cardService.GetAllSets().ToArray();
            foreach (var set in sets)
            {
                var baseCards = _cardService.GetAllBaseBySet(set.Id).ToArray();
                if (baseCards.Length == 0) continue;

                var baseCardIds = baseCards.Select(it => it.BaseId).Distinct().ToArray();
                var priceGroups = _cardPriceRepository.GetPrices(baseCardIds)
                    .ToDictionary(pg => pg.CardId, pg => pg);

                double totalPrice = 0;
                int priceCount = 0;

                foreach (var card in baseCards)
                {
                    if (!priceGroups.TryGetValue(card.BaseId, out var priceGroup)) continue;

                    var price = priceGroup.GetByVariant(card.VariantId);
                    if (price == null) continue;

                    totalPrice += price.MainPrice;
                    priceCount++;
                }

                if (priceCount == 0) continue;

                _setPriceRepository.InsertPrice(set.Id, totalPrice);
            }

            _logger.LogInformation("Finished set price sync");
        }
    }
}
