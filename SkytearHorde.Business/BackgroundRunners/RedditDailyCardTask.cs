using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using PdfSharpCore.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using System.Text;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Extensions;

namespace SkytearHorde.Business.BackgroundRunners
{
    public class RedditDailyCardTask : RecurringHostedServiceBase
    {
        private readonly CardService _cardService;
        private readonly CardPageService _cardPageService;
        private readonly SettingsService _settingsService;
        private readonly IAbilityFormatter _abilityFormatter;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISiteService _siteService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly RedditDailyCardRepository _redditDailyCardRepository;
        private readonly ILogger<RedditDailyCardTask> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly Random _random;

        public RedditDailyCardTask(
            CardService cardService,
            CardPageService cardPageService,
            SettingsService settingsService,
            IAbilityFormatter abilityFormatter,
            IUmbracoContextFactory umbracoContextFactory,
            ISiteService siteService,
            ISiteAccessor siteAccessor,
            RedditDailyCardRepository redditDailyCardRepository,
            ILogger<RedditDailyCardTask> logger,
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment webHostEnvironment) : base(logger, TimeSpan.FromDays(1), DateHelper.GetToNextTime(11))
        {
            _cardService = cardService;
            _cardPageService = cardPageService;
            _settingsService = settingsService;
            _abilityFormatter = abilityFormatter;
            _umbracoContextFactory = umbracoContextFactory;
            _siteService = siteService;
            _siteAccessor = siteAccessor;
            _redditDailyCardRepository = redditDailyCardRepository;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _webHostEnvironment = webHostEnvironment;
            _random = new Random();
        }

        public override async Task PerformExecuteAsync(object? state)
        {
            try
            {
                using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
                foreach (var siteId in _siteService.GetAllSites())
                {
                    _siteAccessor.SetSiteId(siteId);

                    await DoRedditSubmit();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong");
            }
        }

        private async Task DoRedditSubmit()
        {
            var settings = _settingsService.GetSiteSettings();
            if (settings.RedditSettings is null || !settings.RedditSettings.Enabled) return;

            var cardsAlreadyChosen = _redditDailyCardRepository.GetCards();

            var validSets = _cardService.GetAllSets().Where(it => it.HasBeenReleased);
            var cards = validSets.SelectMany(set => _cardService.GetAllBySet(set.Id)).Where(it => !cardsAlreadyChosen.Contains(it.BaseId)).ToArray();
            if (cards.Length == 0) return;

            var selectedCard = cards[_random.Next(0, cards.Length)];
            var cardAttributes = selectedCard.Attributes.ToDictionary(it => it.Key, it => it);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"[{selectedCard.DisplayName}](https://sw-unlimited-db.com{_cardPageService.GetUrl(selectedCard)}) ([Image](https://sw-unlimited-db.com{selectedCard.Image?.Url()}))");

            var cardSettings = _settingsService.GetCardSettings();
            foreach (var displayItem in cardSettings.Display.ToItems<IPublishedElement>().OfType<CardDetailAbilityDisplay>())
            {
                var ability = displayItem.Ability as CardAttribute;
                if (!cardAttributes.TryGetValue(ability?.Name, out var cardValue))
                {
                    continue;
                }

                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"* **{ability.DisplayName}**: {_abilityFormatter.TranslateSpecialCharsToReddit(cardValue.Value.GetAbilityValue())}");
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("^(This bot is maintained by [Patrick](https://www.reddit.com/user/Patrickdemooij9).)");

            var path = System.IO.Path.Combine($"{_webHostEnvironment.WebRootPath}\\{selectedCard.Image?.Url()}");

            byte[] bytes = [];
            if (System.IO.File.Exists(path))
            {
                bytes = System.IO.File.ReadAllBytes(path);
            }

            var redditClient = new RedditApiClient(
                _httpClientFactory.CreateClient(),
                settings.RedditSettings.Username,
                settings.RedditSettings.Password,
                settings.RedditSettings.ClientId,
                settings.RedditSettings.ClientSecret);
            await redditClient.SubmitPostAsync(
                settings.RedditSettings.Subreddit,
                $"[COTD] {selectedCard.DisplayName}",
                stringBuilder.ToString(),
                "533a38e2-ee74-11ed-b8df-469bc4a5ba72",
                bytes);

            _redditDailyCardRepository.AddCard(selectedCard.BaseId);
        }
    }
}
