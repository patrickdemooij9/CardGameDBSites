using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class DeckVersionRepository
    {
        private readonly IScopeProvider _scopeProvider;
        //private readonly Cache.IRepositoryCachePolicy<Deck, int> _cachePolicy;

        public DeckVersionRepository(IScopeProvider scopeProvider, IAppPolicyCache cache)
        {
            _scopeProvider = scopeProvider;

            //var policyOptions = new Cache.RepositoryCachePolicyOptions();
            //_cachePolicy = new Cache.DefaultRepositoryCachePolicy<Deck, int>(cache, policyOptions);
        }

        /*public DeckVersion? GetLatest(int deckId, DeckStatus status)
        {
            using var scope = _scopeProvider.CreateScope();

            var isPublished = status == DeckStatus.Published;
            scope.Database.FirstOrDefault<DeckVersionDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DeckVersionDBModel>()
                .Where<DeckVersionDBModel>(it => it.DeckId == deckId && it.Published == isPublished && it.IsCurrent));
        }*/

        private DeckVersion Map(DeckVersionDBModel dbModel)
        {
            return new DeckVersion
            {
                Id = dbModel.Id,
                Cards = dbModel.DeckCards.Select(it => new DeckCard(it.CardId, it.GroupId, it.SlotId, it.Amount)).ToList()
            };
        }
    }
}
