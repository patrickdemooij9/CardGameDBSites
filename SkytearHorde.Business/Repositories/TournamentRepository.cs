using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database;
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

        public Guid Create(TournamentEvent tournament)
        {
            using var scope = _scopeProvider.CreateScope();

            var model = new TournamentEventDBModel
            {
                Id = tournament.Id == Guid.Empty ? Guid.NewGuid() : tournament.Id,
                SiteId = tournament.SiteId,
                Name = tournament.Name,
                Date = tournament.Date,
                FormatId = tournament.FormatId,
                PlayerCount = tournament.PlayerCount,
                SourceUrl = tournament.SourceUrl,
                CreatedAt = DateTime.UtcNow
            };

            scope.Database.Insert(model);
            scope.Complete();

            return model.Id;
        }

        public TournamentEvent? Get(Guid id)
        {
            using var scope = _scopeProvider.CreateScope();

            var model = scope.Database.FirstOrDefault<TournamentEventDBModel>(
                scope.SqlContext.Sql()
                    .SelectAll()
                    .From<TournamentEventDBModel>()
                    .Where<TournamentEventDBModel>(t => t.Id == id));

            return model is null ? null : Map(model);
        }

        public List<TournamentEvent> GetBySite(int siteId, int? formatId = null)
        {
            using var scope = _scopeProvider.CreateScope();

            var sql = scope.SqlContext.Sql()
                .SelectAll()
                .From<TournamentEventDBModel>()
                .Where<TournamentEventDBModel>(t => t.SiteId == siteId);

            if (formatId.HasValue)
            {
                var fid = formatId.Value;
                sql = sql.Where<TournamentEventDBModel>(t => t.FormatId == fid);
            }

            return scope.Database.Fetch<TournamentEventDBModel>(sql).Select(Map).ToList();
        }

        private static TournamentEvent Map(TournamentEventDBModel model) => new TournamentEvent
        {
            Id = model.Id,
            SiteId = model.SiteId,
            Name = model.Name,
            Date = model.Date,
            FormatId = model.FormatId,
            PlayerCount = model.PlayerCount,
            SourceUrl = model.SourceUrl,
            CreatedAt = model.CreatedAt
        };
    }
}
