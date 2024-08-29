using Umbraco.Cms.Core.Packaging;

namespace SkytearHorde.Content
{
    public class ContentMigrationPlan : PackageMigrationPlan
    {
        public ContentMigrationPlan() : base("ContentMigration")
        {
        }

        protected override void DefinePlan()
        {
            To("AD5548C6-4923-4CF1-BCDE-1769A6B341AD");
            // Uncomment after first install steps
            //To<SkytearHordeMigration>("AD5548C6-4923-4CF1-BCDE-1769A6B341AF");
        }
    }
}
