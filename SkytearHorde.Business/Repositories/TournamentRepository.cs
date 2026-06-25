using SkytearHorde.Entities.Models.Business.Tournament;
using SkytearHorde.Entities.Models.Database.Tournament;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class TournamentRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public TournamentRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public void Save(Tournament tournament)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);

            if (tournament.Id == 0)
            {
                var tournamentDBModel = Map(tournament);
                scope.Database.Insert(tournamentDBModel);
                tournament.Id = tournamentDBModel.Id; // Update the ID after insertion

            }
            else
            {
                var tournamentDBModel = Map(tournament);
                scope.Database.Update(tournamentDBModel);
            }
        }

        public Tournament? GetById(int id)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var tournamentDBModel = scope.Database.FirstOrDefault<TournamentDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<TournamentDBModel>()
                .Where<TournamentDBModel>(x => x.Id == id));
            return tournamentDBModel != null ? Map(tournamentDBModel) : null;
        }

        private TournamentDBModel Map(Tournament tournament)
        {
            return new TournamentDBModel
            {
                Id = tournament.Id,
                Name = tournament.Name,
                FormatId = tournament.FormatId,
                Type = tournament.Type,
                DateUtc = tournament.DateUtc,
                Source = tournament.Source,
                ExternalUrl = tournament.ExternalUrl,
                ExternalId = tournament.ExternalId
            };
        }

        private Tournament Map(TournamentDBModel tournamentDBModel)
        {
            return new Tournament
            {
                Id = tournamentDBModel.Id,
                Name = tournamentDBModel.Name,
                FormatId = tournamentDBModel.FormatId,
                Type = tournamentDBModel.Type,
                DateUtc = tournamentDBModel.DateUtc,
                Source = tournamentDBModel.Source,
                ExternalUrl = tournamentDBModel.ExternalUrl,
                ExternalId = tournamentDBModel.ExternalId
            };
        }

        // TournamentEntrant

        public void Save(TournamentEntrant entrant)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);

            if (entrant.Id == 0)
            {
                var dbModel = Map(entrant);
                scope.Database.Insert(dbModel);
                entrant.Id = dbModel.Id;
            }
            else
            {
                scope.Database.Update(Map(entrant));
            }
        }

        public TournamentEntrant? GetEntrantById(int id)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var dbModel = scope.Database.FirstOrDefault<TournamentEntrantDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<TournamentEntrantDBModel>()
                .Where<TournamentEntrantDBModel>(x => x.Id == id));
            return dbModel != null ? Map(dbModel) : null;
        }

        public IEnumerable<TournamentEntrant> GetEntrantsByTournamentId(int tournamentId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var dbModels = scope.Database.Fetch<TournamentEntrantDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<TournamentEntrantDBModel>()
                .Where<TournamentEntrantDBModel>(x => x.TournamentId == tournamentId));
            return dbModels.Select(Map);
        }

        private TournamentEntrantDBModel Map(TournamentEntrant entrant)
        {
            return new TournamentEntrantDBModel
            {
                Id = entrant.Id,
                TournamentId = entrant.TournamentId,
                PlayerName = entrant.PlayerName,
                Placement = entrant.Placement,
                Wins = entrant.Wins,
                Losses = entrant.Losses,
                Draws = entrant.Draws,
                ExternalId = entrant.ExternalId,
                Source = entrant.Source,
                TournamentDeckId = entrant.TournamentDeckId
            };
        }

        private TournamentEntrant Map(TournamentEntrantDBModel dbModel)
        {
            return new TournamentEntrant
            {
                Id = dbModel.Id,
                TournamentId = dbModel.TournamentId,
                PlayerName = dbModel.PlayerName,
                Placement = dbModel.Placement,
                Wins = dbModel.Wins,
                Losses = dbModel.Losses,
                Draws = dbModel.Draws,
                ExternalId = dbModel.ExternalId,
                Source = dbModel.Source,
                TournamentDeckId = dbModel.TournamentDeckId
            };
        }

        // TournamentRound

        public void Save(TournamentRound round)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);

            if (round.Id == 0)
            {
                var dbModel = Map(round);
                scope.Database.Insert(dbModel);
                round.Id = dbModel.Id;
            }
            else
            {
                scope.Database.Update(Map(round));
            }
        }

        public TournamentRound? GetRoundById(int id)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var dbModel = scope.Database.FirstOrDefault<TournamentRoundDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<TournamentRoundDBModel>()
                .Where<TournamentRoundDBModel>(x => x.Id == id));
            return dbModel != null ? Map(dbModel) : null;
        }

        public IEnumerable<TournamentRound> GetRoundsByTournamentId(int tournamentId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var dbModels = scope.Database.Fetch<TournamentRoundDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<TournamentRoundDBModel>()
                .Where<TournamentRoundDBModel>(x => x.TournamentId == tournamentId));
            return dbModels.Select(Map);
        }

        private TournamentRoundDBModel Map(TournamentRound round)
        {
            return new TournamentRoundDBModel
            {
                Id = round.Id,
                TournamentId = round.TournamentId,
                RoundNumber = round.RoundNumber,
                Type = (short)round.Type
            };
        }

        private TournamentRound Map(TournamentRoundDBModel dbModel)
        {
            return new TournamentRound
            {
                Id = dbModel.Id,
                TournamentId = dbModel.TournamentId,
                RoundNumber = dbModel.RoundNumber,
                Type = (RoundType)dbModel.Type
            };
        }

        // TournamentMatch

        public void Save(TournamentMatch match)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);

            if (match.Id == 0)
            {
                var dbModel = Map(match);
                scope.Database.Insert(dbModel);
                match.Id = dbModel.Id;
            }
            else
            {
                scope.Database.Update(Map(match));
            }
        }

        public TournamentMatch? GetMatchById(int id)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var dbModel = scope.Database.FirstOrDefault<TournamentMatchDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<TournamentMatchDBModel>()
                .Where<TournamentMatchDBModel>(x => x.Id == id));
            return dbModel != null ? Map(dbModel) : null;
        }

        public IEnumerable<TournamentMatch> GetMatchesByRoundId(int roundId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var dbModels = scope.Database.Fetch<TournamentMatchDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<TournamentMatchDBModel>()
                .Where<TournamentMatchDBModel>(x => x.RoundId == roundId));
            return dbModels.Select(Map);
        }

        private TournamentMatchDBModel Map(TournamentMatch match)
        {
            return new TournamentMatchDBModel
            {
                Id = match.Id,
                RoundId = match.RoundId,
                Entrant1Id = match.Entrant1Id,
                Entrant2Id = match.Entrant2Id,
                WinnerEntrantId = match.WinnerEntrantId,
                GamesWonP1 = match.GamesWonP1,
                GamesWonP2 = match.GamesWonP2
            };
        }

        private TournamentMatch Map(TournamentMatchDBModel dbModel)
        {
            return new TournamentMatch
            {
                Id = dbModel.Id,
                RoundId = dbModel.RoundId,
                Entrant1Id = dbModel.Entrant1Id,
                Entrant2Id = dbModel.Entrant2Id,
                WinnerEntrantId = dbModel.WinnerEntrantId,
                GamesWonP1 = dbModel.GamesWonP1,
                GamesWonP2 = dbModel.GamesWonP2
            };
        }

        // Queries for Meta page

        public IEnumerable<Tournament> GetRecent(int count)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var dbModels = scope.Database.Fetch<TournamentDBModel>(
                scope.SqlContext.Sql()
                    .SelectAll()
                    .From<TournamentDBModel>()
                    .OrderByDescending<TournamentDBModel>(x => x.DateUtc)
                    .Append($"OFFSET 0 ROWS FETCH NEXT {count} ROWS ONLY"));
            return dbModels.Select(Map);
        }

        public int GetPlayerCount(int tournamentId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM TournamentEntrants WHERE TournamentId = @0", tournamentId);
        }

        public IEnumerable<TournamentEntrantSummary> GetTop8Entrants(int tournamentId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.Fetch<TournamentEntrantSummary>(
                "SELECT te.Id, te.TournamentId, te.PlayerName, te.Placement, te.TournamentDeckId, dv.Name AS DeckName " +
                "FROM TournamentEntrants te " +
                "LEFT JOIN Deck d ON d.Id = te.TournamentDeckId " +
                "LEFT JOIN DeckVersion dv ON dv.DeckId = d.Id AND dv.IsCurrent = 1 " +
                "WHERE te.TournamentId = @0 AND te.Placement BETWEEN 1 AND 8 " +
                "ORDER BY te.Placement ASC",
                tournamentId);
        }

        // Meta page aggregations.
        // A deck's "leader" is the card in the leader group/slot (e.g. GroupId 1, SlotId 0).
        // The group/slot is passed in so it can vary per game type.

        public IEnumerable<MetaWinningDeckRow> GetRecentWinningDecks(int count, int leaderGroupId, int leaderSlotId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.Fetch<MetaWinningDeckRow>(
                "SELECT TOP(@0) " +
                "  t.Id AS TournamentId, t.Name AS TournamentName, t.DateUtc AS TournamentDateUtc, t.ExternalUrl AS ExternalUrl, " +
                "  te.PlayerName AS PlayerName, te.TournamentDeckId AS DeckId, dv.Name AS DeckName, lc.CardId AS LeaderCardId " +
                "FROM TournamentEntrants te " +
                "INNER JOIN Tournaments t ON t.Id = te.TournamentId " +
                "LEFT JOIN Deck d ON d.Id = te.TournamentDeckId " +
                "LEFT JOIN DeckVersion dv ON dv.DeckId = d.Id AND dv.IsCurrent = 1 " +
                "OUTER APPLY (SELECT TOP(1) dc.CardId FROM DeckCard dc " +
                "             WHERE dc.VersionId = dv.Id AND dc.GroupId = @1 AND dc.SlotId = @2) lc " +
                "WHERE te.Placement = 1 " +
                "ORDER BY t.DateUtc DESC",
                count, leaderGroupId, leaderSlotId);
        }

        public IEnumerable<MetaLeaderRow> GetTopLeaders(DateTime from, int leaderGroupId, int leaderSlotId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.Fetch<MetaLeaderRow>(
                "SELECT lc.CardId AS LeaderCardId, " +
                "  SUM(CASE WHEN te.Placement = 1 THEN 1 ELSE 0 END) AS Wins, " +
                "  COUNT(*) AS Top8Count " +
                "FROM TournamentEntrants te " +
                "INNER JOIN Tournaments t ON t.Id = te.TournamentId " +
                "INNER JOIN Deck d ON d.Id = te.TournamentDeckId " +
                "INNER JOIN DeckVersion dv ON dv.DeckId = d.Id AND dv.IsCurrent = 1 " +
                "INNER JOIN DeckCard lc ON lc.VersionId = dv.Id AND lc.GroupId = @1 AND lc.SlotId = @2 " +
                "WHERE te.Placement BETWEEN 1 AND 8 AND t.DateUtc >= @0 " +
                "GROUP BY lc.CardId " +
                "ORDER BY Wins DESC, Top8Count DESC",
                from, leaderGroupId, leaderSlotId);
        }

        public int GetWinningDeckCount(DateTime from)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.ExecuteScalar<int>(
                "SELECT COUNT(DISTINCT dv.Id) " +
                "FROM TournamentEntrants te " +
                "INNER JOIN Tournaments t ON t.Id = te.TournamentId " +
                "INNER JOIN Deck d ON d.Id = te.TournamentDeckId " +
                "INNER JOIN DeckVersion dv ON dv.DeckId = d.Id AND dv.IsCurrent = 1 " +
                "WHERE te.Placement = 1 AND t.DateUtc >= @0",
                from);
        }

        public IEnumerable<MetaPopularCardRow> GetPopularCardsInWinningDecks(DateTime from, int leaderGroupId, int leaderSlotId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.Fetch<MetaPopularCardRow>(
                "SELECT dc.CardId AS CardId, COUNT(DISTINCT dv.Id) AS DeckCount " +
                "FROM TournamentEntrants te " +
                "INNER JOIN Tournaments t ON t.Id = te.TournamentId " +
                "INNER JOIN Deck d ON d.Id = te.TournamentDeckId " +
                "INNER JOIN DeckVersion dv ON dv.DeckId = d.Id AND dv.IsCurrent = 1 " +
                "INNER JOIN DeckCard dc ON dc.VersionId = dv.Id AND dc.GroupId <> 99 " +
                "WHERE te.Placement = 1 AND t.DateUtc >= @0 " +
                "  AND NOT (dc.GroupId = @1 AND dc.SlotId = @2) " +
                "GROUP BY dc.CardId " +
                "ORDER BY DeckCount DESC",
                from, leaderGroupId, leaderSlotId);
        }
    }

    // Raw fetch rows for the meta aggregations. Card ids are resolved to names in the service layer.
    public class MetaWinningDeckRow
    {
        public int TournamentId { get; set; }
        public string TournamentName { get; set; } = string.Empty;
        public DateTime TournamentDateUtc { get; set; }
        public string? ExternalUrl { get; set; }
        public string? PlayerName { get; set; }
        public int DeckId { get; set; }
        public string? DeckName { get; set; }
        public int? LeaderCardId { get; set; }
    }

    public class MetaLeaderRow
    {
        public int LeaderCardId { get; set; }
        public int Wins { get; set; }
        public int Top8Count { get; set; }
    }

    public class MetaPopularCardRow
    {
        public int CardId { get; set; }
        public int DeckCount { get; set; }
    }
}
