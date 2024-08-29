using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class DeckCalculateScoreRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public DeckCalculateScoreRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public void ScheduleDeckCalculate(int deckId, DateTime date)
        {
            using var scope = _scopeProvider.CreateScope();
            var currentRecord = scope.Database.FirstOrDefault<DeckCalculateScoreDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DeckCalculateScoreDBModel>()
                .Where<DeckCalculateScoreDBModel>(it => it.DeckId == deckId));
            if (currentRecord is null)
            {
                scope.Database.Insert(new DeckCalculateScoreDBModel { DeckId = deckId, NextCalculateDate = date });
            }
            else
            {
                currentRecord.NextCalculateDate = date;
                scope.Database.Update(currentRecord);
            }
            scope.Complete();
        }

        public void RemoveEntry(int deckId)
        {
            using var scope = _scopeProvider.CreateScope();

            var entry = scope.Database.FirstOrDefault<DeckCalculateScoreDBModel>(scope.SqlContext.Sql()
                .SelectAll().From<DeckCalculateScoreDBModel>().Where<DeckCalculateScoreDBModel>(it => it.DeckId == deckId));
            if (entry is null) return;
            scope.Database.Delete(entry);

            scope.Complete();
        }

        public DeckCalculateRequest[] GetDecksToProcess()
        {
            //TODO: Ideally we don't need to know the siteID here....

            var date = DateTime.UtcNow;
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.Fetch<DeckCalculateRequest>(scope.SqlContext.Sql()
                .Select("DeckId,SiteId")
                .From<DeckCalculateScoreDBModel>()
                .LeftJoin<DeckDBModel>().On<DeckCalculateScoreDBModel, DeckDBModel>((left, right) => left.DeckId == right.Id)
                .Where<DeckCalculateScoreDBModel>(it => it.NextCalculateDate <= date)).Take(500).ToArray();
        }
    }
}
