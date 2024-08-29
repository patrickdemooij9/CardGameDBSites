using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class CollectionPackMigration : MigrationBase
    {
        public CollectionPackMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("CollectionPack"))
            {
                Create.Table<CollectionPackDBModel>().Do();
            }
        }
    }
}
