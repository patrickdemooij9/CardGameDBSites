using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Business.Config;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Services
{
    public class SettingsService
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISiteService _siteService;

        public SettingsService(IUmbracoContextFactory umbracoContextFactory, ISiteService siteService)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _siteService = siteService;
        }

        public SiteSettingsConfig GetSiteSettings()
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var settings = _siteService.GetRoot().FirstChild<Settings>()?.FirstChild<SiteSettings>();
            if (settings is null) return new SiteSettingsConfig() { MainColor = "#ffffff", HoverMainColor = "#f0f0f0", BorderColor = "#000000"};

            return new SiteSettingsConfig
            {
                MainColor = settings.MainColor,
                HoverMainColor = settings.HoverMainColor,
                BorderColor = settings.BorderColor,
                Keywords = settings.KeywordImages.ToItems<KeywordImage>().Select(it => new KeywordImageConfig(it.Keyword, it.Image?.Url())
                {
                    DiscordIcon = $"<:{it.DiscordIconName}:{it.DiscordIconID}>"
                }).ToArray(),
                AllowPricing = settings.AllowPricingSync,
                RedditSettings = new RedditSettingsConfig
                {
                    Enabled = settings.AllowRedditIntegration,
                    Username = settings.RedditUsername,
                    Password = settings.RedditPassword,
                    ClientId = settings.RedditClientID,
                    ClientSecret = settings.RedditClientSecret,
                    Subreddit = settings.RedditSubreddit
                },
                SortOptions = settings.SortOptions.ToItems<SortOption>().ToArray()
            };
        }

        public DeckSettings GetDeckSettings()
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            return _siteService.GetRoot().FirstChild<Settings>().FirstChild<DeckSettings>();
        }

        public DeckTypeModel[] GetTypes()
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            return _siteService.GetSettings().Children<SquadSettings>()?.Select(it => new DeckTypeModel
            {
                Id = it.TypeID,
                DisplayName = it.TypeDisplayName!
            }).ToArray() ?? Array.Empty<DeckTypeModel>();
        }

        public SquadSettings GetSquadSettings()
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            return _siteService.GetRoot().FirstChild<Settings>().FirstChild<SquadSettings>();
        }

        public SquadSettings GetSquadSettings(int typeId)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            return _siteService.GetRoot().FirstChild<Settings>().FirstChild<SquadSettings>(it => it.TypeID == typeId);
        }

        public CardSettings GetCardSettings()
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            return _siteService.GetRoot().FirstChild<Settings>().FirstChild<CardSettings>();
        }

        public DiscordSettingsConfig? GetDiscordSettings()
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var settings = _siteService.GetRoot().FirstChild<Settings>()?.FirstChild<DiscordSettings>();
            if (settings is null) return null;
            return new DiscordSettingsConfig { Token = settings.Token!, BaseUrl = settings.BaseUrl!, FooterText = settings.EmbedFooterText };
        }

        public CollectionSettingsConfig GetCollectionSettings()
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var settings = _siteService.GetRoot().FirstChild<Settings>()?.FirstChild<CollectionSettings>();
            if (settings is null) return new CollectionSettingsConfig();
            return new CollectionSettingsConfig
            {
                AllowCardCollecting = settings.AllowCardCollecting,
                AllowSetCollecting = settings.AllowSetCollecting,
                MainIdentifier = settings.MainIdentifier?.Name!
            };
        }
    }
}
