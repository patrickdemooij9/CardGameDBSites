using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class CardSetReferenceMigration : MigrationBase
    {
        private readonly ISiteService _siteService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly CardService _cardService;
        private readonly IContentService _contentService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        public CardSetReferenceMigration(IMigrationContext context,
            ISiteService siteService,
            ISiteAccessor siteAccessor,
            CardService cardService,
            IContentService contentService,
            IUmbracoContextFactory umbracoContextFactory,
            IPublishedSnapshotAccessor publishedSnapshotAccessor) : base(context)
        {
            _siteService = siteService;
            _siteAccessor = siteAccessor;
            _cardService = cardService;
            _contentService = contentService;
            _umbracoContextFactory = umbracoContextFactory;
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
        }

        protected override void Migrate()
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

            var allBaseVariants = ctx.UmbracoContext.Content.GetByContentType(CardVariant.GetModelContentType(_publishedSnapshotAccessor)).OfType<CardVariant>().Where(it => it.Set != null && it.VariantType is null).GroupBy(it => it.Set?.Id).ToDictionary(it => it.Key, it => it.ToArray());
            foreach (var site in _siteService.GetAllSites())
            {
                _siteAccessor.SetSiteId(site);

                var sortOptions = _siteService.GetSettings().FirstChild<SiteSettings>()?.SortOptions.ToItems<SortOption>().ToArray();
                foreach (var set in _cardService.GetAllSets())
                {
                    if (!allBaseVariants.ContainsKey(set.Id)) continue;

                    var cards = allBaseVariants[set.Id].ToArray();

                    if (sortOptions?.Length > 0)
                    {
                        var cardsDict = cards.ToDictionary(it => it.Id, it => it);
                        var sortingHelper = new SortingHelper(cards.Select(it => _cardService.Get(it.Id)).WhereNotNull());

                        var sortedCards = sortingHelper.Sort(sortOptions);
                        cards = sortedCards.Select(it => cardsDict[it.VariantId]).ToArray();
                    }

                    var contentItem = _contentService.GetById(set.Id)!;
                    contentItem.SetValue("cards", string.Join(',', cards.Select(it => Udi.Create(Constants.UdiEntityType.Document, it.Key))));
                    _contentService.SaveAndPublish(contentItem);
                }
            }
        }
    }
}
