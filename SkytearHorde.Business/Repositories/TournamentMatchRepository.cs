using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class TournamentMatchRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public TournamentMatchRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public Guid Add(TournamentMatch match)
        {
            using var scope = _scopeProvider.CreateScope();

            var model = new TournamentMatchDBModel
            {
                Id = match.Id == Guid.Empty ? Guid.NewGuid() : match.Id,
                TournamentEventId = match.TournamentEventId,
                TournamentEntrantId = match.TournamentEntrantId,
                RoundNumber = match.RoundNumber,
                OpponentName = match.OpponentName,
                Wins = match.Wins,
                Losses = match.Losses,
                Draws = match.Draws,
                CreatedAt = match.CreatedAt == default ? DateTime.UtcNow : match.CreatedAt,
                ExternalId = match.ExternalId
            };

            scope.Database.Insert(model);
            scope.Complete();

            return model.Id;
        }

        public void Update(TournamentMatch match)
        {
            using var scope = _scopeProvider.CreateScope();

            var model = scope.Database.FirstOrDefault<TournamentMatchDBModel>(
                scope.SqlContext.Sql()
                    .SelectAll()
                    .From<TournamentMatchDBModel>()
                    .Where<TournamentMatchDBModel>(m => m.Id == match.Id));

            if (model is null) return;

            model.RoundNumber = match.RoundNumber;
            model.OpponentName = match.OpponentName;
            model.Wins = match.Wins;
            model.Losses = match.Losses;
            model.Draws = match.Draws;
            scope.Database.Update(model);
            scope.Complete();
        }

        public TournamentMatch? Get(Guid id)
        {
            using var scope = _scopeProvider.CreateScope();

            var model = scope.Database.FirstOrDefault<TournamentMatchDBModel>(
                scope.SqlContext.Sql()
                    .SelectAll()
                    .From<TournamentMatchDBModel>()
                    .Where<TournamentMatchDBModel>(m => m.Id == id));

            return model is null ? null : Map(model);
        }

        public List<TournamentMatch> GetByEntrant(Guid entrantId)
        {
            using var scope = _scopeProvider.CreateScope();

            return scope.Database.Fetch<TournamentMatchDBModel>(
                scope.SqlContext.Sql()
                    .SelectAll()
                    .From<TournamentMatchDBModel>()
                    .Where<TournamentMatchDBModel>(m => m.TournamentEntrantId == entrantId))
                .Select(Map)
                .ToList();
        }

        public List<TournamentMatch> GetByEntrants(IEnumerable<Guid> entrantIds)
        {
            var ids = entrantIds.ToArray();
            if (ids.Length == 0) return [];

            using var scope = _scopeProvider.CreateScope();
            return scope.Database.Fetch<TournamentMatchDBModel>(
                scope.SqlContext.Sql()
                    .SelectAll()
                    .From<TournamentMatchDBModel>()
                    .WhereIn<TournamentMatchDBModel>(m => m.TournamentEntrantId, ids))
                .Select(Map)
                .ToList();
        }

        public List<TournamentMatch> GetByTournament(Guid tournamentEventId)
        {
            using var scope = _scopeProvider.CreateScope();

            return scope.Database.Fetch<TournamentMatchDBModel>(
                scope.SqlContext.Sql()
                    .SelectAll()
                    .From<TournamentMatchDBModel>()
                    .Where<TournamentMatchDBModel>(m => m.TournamentEventId == tournamentEventId))
                .Select(Map)
                .ToList();
        }

        public void Delete(Guid id)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Execute("DELETE FROM TournamentMatch WHERE Id = @0", id);
            scope.Complete();
        }

        public void DeleteByEntrant(Guid entrantId)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Execute("DELETE FROM TournamentMatch WHERE TournamentEntrantId = @0", entrantId);
            scope.Complete();
        }

        public void DeleteByTournament(Guid tournamentEventId)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Execute("DELETE FROM TournamentMatch WHERE TournamentEventId = @0", tournamentEventId);
            scope.Complete();
        }

        private static TournamentMatch Map(TournamentMatchDBModel model) => new TournamentMatch
        {
            Id = model.Id,
            TournamentEventId = model.TournamentEventId,
            TournamentEntrantId = model.TournamentEntrantId,
            RoundNumber = model.RoundNumber,
            OpponentName = model.OpponentName,
            Wins = model.Wins,
            Losses = model.Losses,
            Draws = model.Draws,
            CreatedAt = model.CreatedAt,
            ExternalId = model.ExternalId
        };
    }
}
