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
    }
}
