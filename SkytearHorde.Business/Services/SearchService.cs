using Examine;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Services
{
    public class SearchService
    {
        private readonly IExamineManager _examineManager;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public SearchService(IExamineManager examineManager, IUmbracoContextFactory umbracoContextFactory)
        {
            _examineManager = examineManager;
            _umbracoContextFactory = umbracoContextFactory;
        }

        public Card[] SearchCards(string query, int amount, int siteId)
        {
            if (string.IsNullOrWhiteSpace(query)) return Array.Empty<Card>();

            if (!_examineManager.TryGetIndex("CardIndex", out var index)) return Array.Empty<Card>();

            var ids = index.Searcher
                .CreateQuery()
                .NativeQuery($"+__IndexType:content")
                .And()
                .NodeTypeAlias(Card.ModelTypeAlias)
                .And()
                .Field("nodeName", query)
                .And()
                .Field("siteId", siteId.ToString())
                .Execute()
                .Take(amount)
                .Select(it => int.Parse(it.Id));

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            return ids.Select(ctx.UmbracoContext.Content.GetById).OfType<Card>().ToArray();
        }
    }
}
