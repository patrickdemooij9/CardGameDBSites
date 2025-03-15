using SkytearHorde.Business.Middleware;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Services.Site
{
    public class SiteService : ISiteService
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISiteAccessor _siteAccessor;
        private readonly AppCaches _appCaches;

        public SiteService(IUmbracoContextFactory umbracoContextFactory, ISiteAccessor siteAccessor, AppCaches appCaches)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _siteAccessor = siteAccessor;
            _appCaches = appCaches;
        }

        public int[] GetAllSites()
        {
            var sites = new List<int>();

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            foreach(var homepage in ctx.UmbracoContext.Content.GetAtRoot().OfType<Homepage>())
            {
                var siteSettings = homepage.FirstChild<Settings>()?.FirstChild<SiteSettings>();
                if (siteSettings is null) continue;

                sites.Add(siteSettings.SiteId);
            }
            return sites.ToArray();
        }

        public CardOverview GetCardOverview()
        {
            return GetRoot().FirstChild<CardOverview>()!;
        }

        public DeckOverview GetDeckOverview(int typeId)
        {
            return GetRoot().FirstChild<DeckOverview>(it => ((it.SquadSettings as SquadSettings)?.TypeID ?? 1) == typeId)!;
        }

        public DeckOverview[] GetDeckOverviews()
        {
            return GetRoot().Children<DeckOverview>()?.ToArray() ?? Array.Empty<DeckOverview>();
        }

        public SetOverview? GetSetOverview()
        {
            return GetRoot().FirstChild<SetOverview>();
        }

        public CollectionPage? GetCollectionPage()
        {
            return GetRoot().FirstChild<CollectionPage>();
        }

        public Homepage GetRoot()
        {
            var siteId = _siteAccessor.GetSiteId();

            return _appCaches.RuntimeCache.GetCacheItem($"GetSiteRoot_{siteId}", () =>
            {
                using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

                var siteSettings = ctx.UmbracoContext.Content?.GetAtRoot().OfType<Homepage>().Select(it => it.FirstChild<Settings>()?.FirstChild<SiteSettings>()).FirstOrDefault(it => it?.SiteId == siteId);
                if (siteSettings is null)
                    throw new InvalidOperationException($"Could not find site by id: {siteId}");
                return (Homepage)siteSettings.Root();
            })!;
        }

        public Settings GetSettings()
        {
            return GetRoot().FirstChild<Settings>()!;
        }

        public CardAttribute[] GetAllAttributes()
        {
            return GetRoot().FirstChild<CardAttributeContainer>()?.Children<CardAttribute>()?.ToArray() ?? Array.Empty<CardAttribute>();
        }
    }
}
