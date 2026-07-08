using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class PeriodRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public PeriodRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public void Save(Period period)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);

            if (period.Id == 0)
            {
                var dbModel = Map(period);
                scope.Database.Insert(dbModel);
                period.Id = dbModel.Id;
            }
            else
            {
                scope.Database.Update(Map(period));
            }
        }

        public Period? GetById(int id)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var dbModel = scope.Database.FirstOrDefault<PeriodDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<PeriodDBModel>()
                .Where<PeriodDBModel>(x => x.Id == id));
            return dbModel != null ? Map(dbModel) : null;
        }

        public IEnumerable<Period> GetBySiteAndFormat(int siteId, int formatId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var dbModels = scope.Database.Fetch<PeriodDBModel>(
                "SELECT * FROM Periods WHERE SiteId = @0 AND FormatId = @1 ORDER BY StartingDateUtc DESC",
                siteId, formatId);
            return dbModels.Select(Map);
        }

        public Period? FindMatchingPeriod(int siteId, int formatId, DateTime tournamentDateUtc)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var dbModel = scope.Database.FirstOrDefault<PeriodDBModel>(
                "SELECT * FROM Periods WHERE SiteId = @0 AND FormatId = @1 " +
                "AND StartingDateUtc <= @2 AND (EndDateUtc IS NULL OR EndDateUtc >= @2)",
                siteId, formatId, tournamentDateUtc);
            return dbModel != null ? Map(dbModel) : null;
        }

        private PeriodDBModel Map(Period period)
        {
            return new PeriodDBModel
            {
                Id = period.Id,
                SiteId = period.SiteId,
                FormatId = period.FormatId,
                Name = period.Name,
                StartingDateUtc = period.StartingDateUtc,
                EndDateUtc = period.EndDateUtc
            };
        }

        private Period Map(PeriodDBModel dbModel)
        {
            return new Period
            {
                Id = dbModel.Id,
                SiteId = dbModel.SiteId,
                FormatId = dbModel.FormatId,
                Name = dbModel.Name,
                StartingDateUtc = dbModel.StartingDateUtc,
                EndDateUtc = dbModel.EndDateUtc
            };
        }
    }
}
