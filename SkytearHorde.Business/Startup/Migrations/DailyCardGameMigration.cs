using SkytearHorde.Entities.Models.Database.DailyGame;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DailyCardGameMigration : MigrationBase
    {
        public DailyCardGameMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("DailyCardChallenge"))
            {
                Create.Table<DailyCardChallengeDBModel>().Do();
            }

            if (!TableExists("DailyCardGameSession"))
            {
                Create.Table<DailyCardGameSessionDBModel>().Do();
            }

            if (!TableExists("DailyCardGameGuess"))
            {
                Create.Table<DailyCardGameGuessDBModel>().Do();
            }

            if (!IndexExists("IX_DailyCardChallenge_Site_Date"))
            {
                Database.Execute("CREATE UNIQUE NONCLUSTERED INDEX IX_DailyCardChallenge_Site_Date ON DailyCardChallenge (SiteId, ChallengeDateUtc)");
            }

            if (!IndexExists("IX_DailyCardGameSession_Challenge_Member"))
            {
                Database.Execute("CREATE NONCLUSTERED INDEX IX_DailyCardGameSession_Challenge_Member ON DailyCardGameSession (ChallengeId, MemberId)");
            }

            if (!IndexExists("IX_DailyCardGameSession_Challenge_Status"))
            {
                Database.Execute("CREATE NONCLUSTERED INDEX IX_DailyCardGameSession_Challenge_Status ON DailyCardGameSession (ChallengeId, Status)");
            }

            if (!IndexExists("IX_DailyCardGameGuess_Session_Attempt"))
            {
                Database.Execute("CREATE UNIQUE NONCLUSTERED INDEX IX_DailyCardGameGuess_Session_Attempt ON DailyCardGameGuess (SessionId, AttemptNumber)");
            }
        }
    }
}
