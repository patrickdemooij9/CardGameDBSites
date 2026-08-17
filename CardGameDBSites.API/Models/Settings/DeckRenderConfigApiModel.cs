using SkytearHorde.Entities.Models.Business.Config;

namespace CardGameDBSites.API.Models.Settings
{
    public class DeckRenderConfigApiModel
    {
        public double CardAspect { get; set; } = 0.7;

        public string[] LandscapeTypes { get; set; } = [];

        public string[] BackImageTypes { get; set; } = [];

        public string? TypeAttribute { get; set; }
        public string? CostAttribute { get; set; }
        public string? AspectAttribute { get; set; }
        public Dictionary<string, string> AspectColors { get; set; } = [];
        /// </summary>
        public ArtCropApiModel[] ArtCrops { get; set; } = [];

        /// <summary>Small label above the deck name. Falls back to a derived label when null.</summary>
        public string? Eyebrow { get; set; }

        // ---- Tier thresholds ----------------------------------------------

        /// <summary>Card count at or above which art crops are used, if crops exist.</summary>
        public int TierBMin { get; set; } = 46;

        /// <summary>Card count at or above which the typeset list is forced.</summary>
        public int TierCMin { get; set; } = 141;

        /// <summary>Output is never shorter than width * this. Stops small decks becoming banners.</summary>
        public double MinCanvasRatio { get; set; } = 1.0;

        public DeckRenderConfigApiModel(DeckRenderConfig config)
        {
            CardAspect = config.CardAspect;
            LandscapeTypes = config.LandscapeTypes;
            BackImageTypes = config.BackImageTypes;
            TypeAttribute = config.TypeAttribute;
            CostAttribute = config.CostAttribute;
            AspectAttribute = config.AspectAttribute;
            AspectColors = config.AspectColors;
            ArtCrops = config.ArtCrops.Select(it => new ArtCropApiModel
            {
                TypeValue = it.TypeValue,
                Left = it.Left,
                Top = it.Top,
                Right = it.Right,
                Bottom = it.Bottom
            }).ToArray();
            Eyebrow = config.Eyebrow;
            TierBMin = config.TierBMin;
            TierCMin = config.TierCMin;
            MinCanvasRatio = config.MinCanvasRatio;
        }
    }
}
