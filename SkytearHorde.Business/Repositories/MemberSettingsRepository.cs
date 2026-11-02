using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class MemberSettingsRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public MemberSettingsRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public MemberSettings? Get(int memberId)
        {
            using var scope = _scopeProvider.CreateScope();

            var settings = scope.Database.FirstOrDefault<MemberSettingsDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<MemberSettingsDBModel>()
                .Where<MemberSettingsDBModel>(it => it.MemberId == memberId));

            return settings is null ? null : Map(settings);
        }

        public void Save(MemberSettings settings)
        {
            using var scope = _scopeProvider.CreateScope();

            var model = new MemberSettingsDBModel
            {
                MemberId = settings.MemberId,
                MissingCopiesThreshold = settings.MissingCopiesThreshold
            };

            var exists = scope.Database.ExecuteScalar<int>(scope.SqlContext.Sql()
                .SelectCount()
                .From<MemberSettingsDBModel>()
                .Where<MemberSettingsDBModel>(it => it.MemberId == settings.MemberId)) > 0;

            if (exists)
            {
                scope.Database.Update(model);
            }
            else
            {
                scope.Database.Insert(model);
            }
            scope.Complete();
        }

        private static MemberSettings Map(MemberSettingsDBModel settings)
        {
            return new MemberSettings(settings.MemberId)
            {
                MissingCopiesThreshold = settings.MissingCopiesThreshold
            };
        }
    }
}
