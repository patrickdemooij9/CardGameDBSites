using NPoco;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DuplicateCollectionCardMigration : MigrationBase
    {
        public DuplicateCollectionCardMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            var items = Database.Fetch<DuplicateCollection>(Database.SqlContext.Sql()
                .Select<DuplicateCollection>(it => it.UserId, it => it.VariantId)
                .From<DuplicateCollection>()
                .GroupBy<DuplicateCollection>(it => it.UserId, it => it.VariantId)
                .Append("HAVING COUNT(*) > 1"));

            foreach (var item in items)
            {
                var actualItems = Database.Fetch<CollectionCardDBModel>(Sql()
                    .SelectAll()
                    .From<CollectionCardDBModel>()
                    .Where<CollectionCardDBModel>(it => it.VariantId == item.VariantId && it.UserId == item.UserId));
                if (actualItems.Count == 0) continue;

                var firstItem = actualItems[0];
                firstItem.Amount += actualItems.Skip(1).Sum(it => it.Amount);
                Database.Update(firstItem);
                foreach (var otherItem in actualItems.Skip(1))
                {
                    Database.Delete(otherItem);
                }
            }
        }

        [TableName("CollectionCard")]
        class DuplicateCollection
        {
            [Column("UserId")]
            public int UserId { get; set; }

            [Column("VariantId")]
            public int VariantId { get; set; }
        }
    }
}
