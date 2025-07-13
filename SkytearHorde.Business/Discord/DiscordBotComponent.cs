using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Search;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Discord
{
    public class DiscordBotComponent : IComponent
    {
        private readonly ILogger _logger;
        private readonly CardService _cardService;
        private readonly CardPageService _cardPageService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly SettingsService _settingsService;
        private readonly IOptions<WebRoutingSettings> _settings;
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly ISiteAccessor _siteAccessor;
        private readonly IAbilityFormatter _abilityFormatter;
        private readonly ISiteService _siteService;
        private readonly IRuntimeState _runtimeState;

        public DiscordBotComponent(ILogger<DiscordBot> logger, CardService cardService, CardPageService cardPageService, IUmbracoContextFactory umbracoContextFactory, SettingsService settingsService, IOptions<WebRoutingSettings> settings, IPublishedSnapshotAccessor publishedSnapshotAccessor, ISiteAccessor siteAccessor, IAbilityFormatter abilityFormatter,ISiteService siteService, IRuntimeState runtimeState)
        {
            _logger = logger;
            _cardService = cardService;
            _cardPageService = cardPageService;
            _umbracoContextFactory = umbracoContextFactory;
            _settingsService = settingsService;
            _settings = settings;
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            _siteAccessor = siteAccessor;
            _abilityFormatter = abilityFormatter;
            _siteService = siteService;
            _runtimeState = runtimeState;
            _siteService = siteService;
        }

        public void Initialize()
        {
            if (_runtimeState.Level != RuntimeLevel.Run) return;

            var siteIds = new List<int>();
            using (var ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                var discordSettings = ctx.UmbracoContext.Content?.GetAtRoot().OfType<Homepage>().Select(it => it.FirstChild<Settings>()?.FirstChild<DiscordSettings>()).WhereNotNull() ?? [];
                foreach (var discordSetting in discordSettings)
                {
                    siteIds.Add(discordSetting.Parent!.FirstChild<SiteSettings>()!.SiteId);
                }
            }

            foreach (var siteId in siteIds)
            {
                new DiscordBot(_logger, _cardService, _cardPageService, _umbracoContextFactory, _settingsService, _siteAccessor, _abilityFormatter, _siteService, siteId);
            }
        }

        public void Terminate()
        {
        }
    }
}
