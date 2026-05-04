using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class TournamentEntrantRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public TournamentEntrantRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public Guid Add(TournamentEntrant entrant)
        {
            using var scope = _scopeProvider.CreateScope();

            var model = new TournamentEntrantDBModel
            {
                Id = entrant.Id == Guid.Empty ? Guid.NewGuid() : entrant.Id,
                TournamentEventId = entrant.TournamentEventId,
                PlayerName = entrant.PlayerName,
                Placement = entrant.Placement,
                Wins = entrant.Wins,
                Losses = entrant.Losses,
                Draws = entrant.Draws,
                DeckId = entrant.DeckId,
                ExternalId = entrant.ExternalId
            };

            scope.Database.Insert(model);
            scope.Complete();

            return model.Id;
        }

        public void AddRange(IEnumerable<TournamentEntrant> entrants)
        {
            using var scope = _scopeProvider.CreateScope();

            foreach (var entrant in entrants)
            {
                scope.Database.Insert(new TournamentEntrantDBModel
                {
                    Id = entrant.Id == Guid.Empty ? Guid.NewGuid() : entrant.Id,
                    TournamentEventId = entrant.TournamentEventId,
                    PlayerName = entrant.PlayerName,
                    Placement = entrant.Placement,
                    Wins = entrant.Wins,
                    Losses = entrant.Losses,
                    Draws = entrant.Draws,
                    DeckId = entrant.DeckId,
                    ExternalId = entrant.ExternalId
                });
            }

            scope.Complete();
        }

        public bool ExternalIdExists(Guid tournamentEventId, string externalId)
        {
            using var scope = _scopeProvider.CreateScope();
            return scope.Database.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM TournamentEntrant WHERE TournamentEventId = @0 AND ExternalId = @1",
                tournamentEventId, externalId) > 0;
        }

        public void Update(TournamentEntrant entrant)
        {
            using var scope = _scopeProvider.CreateScope();

            var model = scope.Database.FirstOrDefault<TournamentEntrantDBModel>(
                scope.SqlContext.Sql()
                    .SelectAll()
                    .From<TournamentEntrantDBModel>()
                    .Where<TournamentEntrantDBModel>(e => e.Id == entrant.Id));

            if (model is null) return;

            model.PlayerName = entrant.PlayerName;
            model.Placement = entrant.Placement;
            model.Wins = entrant.Wins;
            model.Losses = entrant.Losses;
            model.Draws = entrant.Draws;
            model.DeckId = entrant.DeckId;

            scope.Database.Update(model);
            scope.Complete();
        }

        public List<TournamentEntrant> GetByTournament(Guid tournamentEventId)
        {
            using var scope = _scopeProvider.CreateScope();

            return scope.Database.Fetch<TournamentEntrantDBModel>(
                scope.SqlContext.Sql()
                    .SelectAll()
                    .From<TournamentEntrantDBModel>()
                    .Where<TournamentEntrantDBModel>(e => e.TournamentEventId == tournamentEventId))
                .Select(Map)
                .ToList();
        }

        public List<TournamentEntrant> GetByDeck(int deckId)
        {
            using var scope = _scopeProvider.CreateScope();

            return scope.Database.Fetch<TournamentEntrantDBModel>(
                scope.SqlContext.Sql()
                    .SelectAll()
                    .From<TournamentEntrantDBModel>()
                    .Where<TournamentEntrantDBModel>(e => e.DeckId == deckId))
                .Select(Map)
                .ToList();
        }

        public TournamentEntrant? Get(Guid id)
        {
            using var scope = _scopeProvider.CreateScope();

            var model = scope.Database.FirstOrDefault<TournamentEntrantDBModel>(
                scope.SqlContext.Sql()
                    .SelectAll()
                    .From<TournamentEntrantDBModel>()
                    .Where<TournamentEntrantDBModel>(e => e.Id == id));

            return model is null ? null : Map(model);
        }

        private static TournamentEntrant Map(TournamentEntrantDBModel model) => new TournamentEntrant
        {
            Id = model.Id,
            TournamentEventId = model.TournamentEventId,
            PlayerName = model.PlayerName,
            Placement = model.Placement,
            Wins = model.Wins,
            Losses = model.Losses,
            Draws = model.Draws,
            DeckId = model.DeckId,
            ExternalId = model.ExternalId
        };
    }
}
