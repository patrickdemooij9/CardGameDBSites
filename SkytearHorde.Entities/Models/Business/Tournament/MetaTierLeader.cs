namespace SkytearHorde.Entities.Models.Business.Tournament
{
    public class MetaTierLeader
    {
        public int CardId { get; set; }
        public string Name { get; set; } = string.Empty;

        public int DeckCount { get; set; }
        public int EventCount { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int Top8Count { get; set; }
        public int FirstPlaceCount { get; set; }

        public double WinratePercentage { get; set; }

        public double WinrateLowerBoundPercentage { get; set; }

        public double MetaSharePercentage { get; set; }

        public double? MetaShareDeltaPoints { get; set; }
        public double? WinrateDeltaPoints { get; set; }
        public bool IsNewEntry { get; set; }

        public MetaTier Tier { get; set; }
        public double TierScore { get; set; }

        public string? UnrankedReason { get; set; }
    }
}
