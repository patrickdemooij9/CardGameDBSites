using Examine;
using Microsoft.Extensions.Logging;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Search;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Models.Business;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Extensions;

namespace SkytearHorde.Business.BackgroundRunners
{
    public class RedditBotTask : RecurringHostedServiceBase
    {
        private readonly SettingsService _settingsService;
        private readonly CardService _cardService;
        private readonly CardPageService _cardPageService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISiteService _siteService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly RedditBotCommentRepository _redditBotCommentRepository;
        private readonly ILogger<RedditBotTask> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private static readonly Regex CardSyntaxPattern = new(@"\[\[([^\]|]+?)(?:\|([^\]]+?))?\]\]", RegexOptions.Compiled);

        public RedditBotTask(
            SettingsService settingsService,
            CardService cardService,
            CardPageService cardPageService,
            IUmbracoContextFactory umbracoContextFactory,
            ISiteService siteService,
            ISiteAccessor siteAccessor,
            RedditBotCommentRepository redditBotCommentRepository,
            ILogger<RedditBotTask> logger,
            IHttpClientFactory httpClientFactory)
            : base(logger, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(1))
        {
            _settingsService = settingsService;
            _cardService = cardService;
            _cardPageService = cardPageService;
            _umbracoContextFactory = umbracoContextFactory;
            _siteService = siteService;
            _siteAccessor = siteAccessor;
            _redditBotCommentRepository = redditBotCommentRepository;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public override async Task PerformExecuteAsync(object? state)
        {
            try
            {
                using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
                foreach (var siteId in _siteService.GetAllSites())
                {
                    _siteAccessor.SetSiteId(siteId);
                    await ProcessSiteAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong in RedditBotTask");
            }
        }

        private async Task ProcessSiteAsync()
        {
            var settings = _settingsService.GetSiteSettings();
            if (settings.RedditSettings is null || !settings.RedditSettings.Enabled) return;

            var baseUrl = "https://sw-unlimited-db.com"; //TODO: Fix correctly

            var redditClient = new RedditApiClient(
                _httpClientFactory.CreateClient(),
                settings.RedditSettings.Username,
                settings.RedditSettings.Password,
                settings.RedditSettings.ClientId,
                settings.RedditSettings.ClientSecret);

            var lastProcessedDate = _redditBotCommentRepository.GetLastProcessedDate();
            var comments = await redditClient.GetNewCommentsAsync(settings.RedditSettings.Subreddit, 25);
            var posts = await redditClient.GetNewPostsAsync(settings.RedditSettings.Subreddit, 25);

            DateTime? latestProcessedDate = null;

            foreach (var comment in comments.OrderBy(it => it.CreatedAt))
            {
                if (lastProcessedDate.HasValue && comment.CreatedAt <= lastProcessedDate.Value) continue;

                if (latestProcessedDate == null || comment.CreatedAt > latestProcessedDate.Value)
                    latestProcessedDate = comment.CreatedAt;

                await TryReplyWithCardsAsync(redditClient, comment.FullName, comment.Body, baseUrl);
            }

            foreach (var post in posts.OrderBy(it => it.CreatedAt))
            {
                if (lastProcessedDate.HasValue && post.CreatedAt <= lastProcessedDate.Value) continue;

                if (latestProcessedDate == null || post.CreatedAt > latestProcessedDate.Value)
                    latestProcessedDate = post.CreatedAt;

                var postBody = string.IsNullOrWhiteSpace(post.SelfText)
                    ? post.Title
                    : $"{post.Title}\n{post.SelfText}";

                await TryReplyWithCardsAsync(redditClient, post.FullName, postBody, baseUrl);
            }

            if (latestProcessedDate.HasValue)
            {
                _redditBotCommentRepository.UpdateLastProcessedDate(latestProcessedDate.Value);
            }
        }

        private async Task TryReplyWithCardsAsync(RedditApiClient redditClient, string parentFullName, string? rawText, string baseUrl)
        {
            var body = rawText?.Replace("\\[", "[").Replace("\\]", "]");
            if (string.IsNullOrWhiteSpace(body)) return;

            var matches = CardSyntaxPattern.Matches(body);
            if (matches.Count == 0) return;

            var cards = new List<Card>();
            foreach (Match match in matches)
            {
                if (cards.Count >= 5) break;

                var cardName = match.Groups[1].Value.Trim();
                var setCode = match.Groups[2].Success ? match.Groups[2].Value.Trim() : null;

                int? setId = null;
                if (!string.IsNullOrEmpty(setCode))
                {
                    var set = _cardService.GetAllSets().FirstOrDefault(it =>
                        it.SetCode?.Equals(setCode, StringComparison.OrdinalIgnoreCase) is true);
                    setId = set?.Id;
                }

                var searchResults = _cardService.Search(new CardSearchQuery(1, _siteAccessor.GetSiteId())
                {
                    Query = cardName,
                    SetId = setId
                }, out _);

                var card = searchResults.FirstOrDefault();
                if (card != null)
                {
                    cards.Add(card);
                }
            }

            if (cards.Count == 0) return;

            var replyText = BuildReplyText(cards, baseUrl);
            await redditClient.ReplyToCommentAsync(parentFullName, replyText);
        }

        private string BuildReplyText(List<Card> cards, string baseUrl)
        {
            var sb = new StringBuilder();

            foreach (var card in cards)
            {
                var cardUrl = $"{baseUrl}{_cardPageService.GetUrl(card)}";
                var imageUrl = card.Image != null ? $"{baseUrl}{card.Image.Url()}" : null;

                if (imageUrl != null)
                {
                    sb.AppendLine($"* **[{card.DisplayName}]({cardUrl})** | [Image]({imageUrl})");
                }
                else
                {
                    sb.AppendLine($"* **[{card.DisplayName}]({cardUrl})**");
                }
            }

            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine("^(Use \\[\\[card name\\]\\] or \\[\\[card name|set code\\]\\] to look up a card.)");

            return sb.ToString();
        }
    }
}
