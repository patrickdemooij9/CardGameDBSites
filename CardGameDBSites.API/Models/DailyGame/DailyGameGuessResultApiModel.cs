namespace CardGameDBSites.API.Models.DailyGame
{
    public class DailyGameGuessResultApiModel
    {
        public required DailyGameBootstrapApiModel State { get; set; }
        public DailyGameAttemptApiModel? LatestAttempt { get; set; }
    }
}
