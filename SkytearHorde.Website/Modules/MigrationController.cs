using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

namespace SkytearHorde.Modules
{
    [PluginController("Migration")]
    public class MigrationController : UmbracoAuthorizedApiController
    {
        private readonly IScopeProvider _scopeProvider;

        public MigrationController(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        [HttpPost]
        public IActionResult RunPriceFixMigration()
        {
            using var scope = _scopeProvider.CreateScope();
            foreach (var entryCardGroup in scope.Database.Query<CardPriceRecordDBModel>(
                scope.SqlContext.Sql()
                .SelectAll()
                .From<CardPriceRecordDBModel>()).GroupBy(it => it.CardId))
            {
                var updatedEntries = new List<CardPriceRecordDBModel>();
                foreach (var entryVariantGroup in entryCardGroup.GroupBy(it => it.VariantId))
                {
                    var firstEntry = entryVariantGroup.OrderBy(it => it.DateUtc).First();
                    var entries = new List<CardPriceRecordDBModel>() { firstEntry };
                    foreach (var entry in entryVariantGroup.Skip(1).OrderBy(it => it.DateUtc))
                    {
                        if (firstEntry.MainPrice == entry.MainPrice && firstEntry.LowestPrice == entry.LowestPrice && firstEntry.HighestPrice == entry.HighestPrice)
                        {
                            scope.Database.Delete(entry);
                            continue;
                        }
                        entries.Add(entry);
                        firstEntry = entry;
                    }

                    var latestEntry = entries.OrderByDescending(it => it.DateUtc).First();
                    latestEntry.IsLatest = true;
                    updatedEntries.Add(latestEntry);
                }

                scope.Database.Execute(scope.Database.SqlContext.Sql($"UPDATE CardPriceRecord SET IsLatest = 0 WHERE CardId = {entryCardGroup.Key}"));
                foreach (var entry in updatedEntries)
                {
                    scope.Database.Update(entry);
                }
            }
            scope.Complete();
            return Ok();
        }
    }
}
