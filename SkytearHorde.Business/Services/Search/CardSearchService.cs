using Examine;
using Examine.Search;
using SkytearHorde.Entities.Models.Business;
using System.Security.Policy;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Services.Search
{
    public class CardSearchService : ICardSearchService
    {
        private readonly IExamineManager _examineManager;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly CardService _cardService;

        public CardSearchService(IExamineManager examineManager, IUmbracoContextFactory umbracoContextFactory,
            CardService cardService)
        {
            _examineManager = examineManager;
            _umbracoContextFactory = umbracoContextFactory;
            _cardService = cardService;
        }

        public Card[] Search(CardSearchQuery query)
        {
            if (!_examineManager.TryGetIndex("ExternalIndex", out var index)) return Array.Empty<Card>();

            var searcher = index.Searcher
                .CreateQuery()
                .NativeQuery($"+__IndexType:content")
                .And()
                .NodeTypeAlias(Entities.Generated.CardVariant.ModelTypeAlias);

            foreach (var filter in query.CustomFields)
            {
                searcher.And().GroupedAnd([$"CustomField.{filter.Key}"], filter.Value.Select(it => it.Replace(" ", "")).ToArray());
            }

            searcher.And().Field("siteId", query.SiteId.ToString());
            if (query.SetId.HasValue)
            {
                searcher.And().Field("CustomField.SetId", query.SetId.Value);
            }
            if (query.VariantTypeId.HasValue)
            {
                searcher.And().Field("VariantType", query.VariantTypeId.Value.ToString());
            }
            if (!query.IncludeHideFromDeck)
            {
                searcher.And().Field("hideFromDecks", "0");
            }

            if (!string.IsNullOrWhiteSpace(query.Query))
            {
                var splitQuery = query.Query.Replace('-', ' ').Split(' ');
                foreach (var queryItem in splitQuery)
                {
                    searcher.And().Field("Name", queryItem.MultipleCharacterWildcard());
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(query.SortBy))
                {
                    searcher.OrderBy(new SortableField("sortOrder", SortType.Int));
                }
                else
                {
                    if (query.SortDescending)
                    {
                        searcher.OrderByDescending(new SortableField(query.SortBy, SortType.Int));
                    }
                    else
                    {
                        searcher.OrderBy(new SortableField(query.SortBy, SortType.Int));
                    }
                }
            }

            var results = searcher.Execute(new QueryOptions(0, query.Amount));
            var ids = results.Select(it => int.Parse(it.Id));

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var allCards = _cardService.GetAll(true).Where(it => it.VariantId > 0).ToDictionary(it => it.VariantId, it => it);
            return ids.Where(allCards.ContainsKey).Select(it => allCards[it]).ToArray();
        }
    }
}
