using CardGameDBSites.API.Models;

namespace CardGameDBSites.API.Models.Meta
{
    public class MetaTierLeaderApiModel
    {
        public int CardId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Tier { get; set; } = string.Empty;

        public int DeckCount { get; set; }
        public int EventCount { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int Top8Count { get; set; }
        public int FirstPlaceCount { get; set; }

        /// <summary>Game win rate, not match win rate.</summary>
        public double WinratePercentage { get; set; }
        public double MetaSharePercentage { get; set; }

        /// <summary>Percentage points vs the previous window. Null when the sample was too thin.</summary>
        public double? MetaShareDeltaPoints { get; set; }
        public double? WinrateDeltaPoints { get; set; }
        public bool IsNewEntry { get; set; }

        public string? UnrankedReason { get; set; }

        public string? MetaUrl { get; set; }
        public ImageCropsApiModel? ImageUrl { get; set; }
    }
}
