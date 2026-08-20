/*using CardGameDBSites.API.Models.Settings;

namespace CardGameDBSites.API.Helpers
{
    /// <summary>
    /// TEMPORARY. Hardcoded deck-render config per site so the client renderer can be built and
    /// exercised before the matching Umbraco document type exists.
    /// <para>
    /// Keyed on the site's base URL because site ids are Umbraco content ids and differ per
    /// environment. When the real document type lands, delete this file and populate
    /// <see cref="DeckRenderConfigApiModel"/> from the published content instead — nothing
    /// outside <c>SettingsController.GetDeckTypeSettings</c> references it.
    /// </para>
    /// </summary>
    public static class MockDeckRenderConfig
    {
        public static DeckRenderConfigApiModel ForSite(string? baseUrl)
        {
            var host = (baseUrl ?? string.Empty).ToLowerInvariant();

            if (host.Contains("shatterpoint")) return Shatterpoint();
            if (host.Contains("sw-unlimited")) return StarWarsUnlimited();
            if (host.Contains("skytear")) return SkytearHorde();

            return Fallback();
        }

        /// <summary>
        /// Star Wars Unlimited. The only game whose decks reach the art-crop tier (Twin Suns
        /// sits at a median of 83 unique cards), so it is the only one with crop bands.
        /// </summary>
        private static DeckRenderConfigApiModel StarWarsUnlimited() => new()
        {
            CardAspect = 0.716,
            LandscapeTypes = ["Base", "Leader"],
            BackImageTypes = ["Leader"],

            TypeAttribute = "Card Type",
            CostAttribute = "Cost",
            AspectAttribute = "Aspects",
            AspectColors = new Dictionary<string, string>
            {
                ["Vigilance"] = "#3B7BBF",
                ["Command"] = "#3E9F4E",
                ["Aggression"] = "#D2232A",
                ["Cunning"] = "#F4B223",
                ["Heroism"] = "#E8E4D4",
                ["Villainy"] = "#4B4453",
            },

            // Units and upgrades put the art below the name plate; events put the rules box in
            // the middle and the art at the BOTTOM, so a single band renders them as text.
            ArtCrops =
            [
                new ArtCropApiModel { TypeValue = "Unit",    Left = 0.055, Top = 0.145, Right = 0.055, Bottom = 0.395 },
                new ArtCropApiModel { TypeValue = "Upgrade", Left = 0.055, Top = 0.130, Right = 0.055, Bottom = 0.380 },
                new ArtCropApiModel { TypeValue = "Event",   Left = 0.055, Top = 0.550, Right = 0.055, Bottom = 0.070 },
                new ArtCropApiModel { TypeValue = "Base",    Left = 0.030, Top = 0.180, Right = 0.030, Bottom = 0.320 },
                new ArtCropApiModel { TypeValue = "Leader",  Left = 0.030, Top = 0.100, Right = 0.030, Bottom = 0.400 },
            ],

            BrandPrefix = "SW",
            BrandAccent = "Unlimited",
            BrandSuffix = "DB",
            AccentColor = "#FFC94A",
        };

        /// <summary>
        /// Shatterpoint. Strike teams are fixed-size (7/10/13) and shaped as N squads of three
        /// plus a mission, so they always land on the full-face tier with the grouped layout.
        /// No crop bands: the art-crop tier is unreachable here anyway.
        /// </summary>
        private static DeckRenderConfigApiModel Shatterpoint() => new()
        {
            CardAspect = 0.669,
            LandscapeTypes = [],
            BackImageTypes = [],

            TypeAttribute = "Unit Type",
            CostAttribute = "Squad Points",
            AspectAttribute = null,

            ArtCrops = [],

            Eyebrow = "Strike Team",
            BrandPrefix = "Shatter",
            BrandAccent = "point",
            BrandSuffix = "DB",
            AccentColor = "#E8913A",
        };

        /// <summary>Skytear Horde. Around 22 unique cards, one flat group.</summary>
        private static DeckRenderConfigApiModel SkytearHorde() => new()
        {
            CardAspect = 0.716,
            TypeAttribute = "Card Type",
            CostAttribute = "Cost",
            ArtCrops = [],

            BrandPrefix = "Skytear",
            BrandAccent = "Horde",
            BrandSuffix = "DB",
            AccentColor = "#59B7D8",
        };

        /// <summary>
        /// Safe default for a site with no config yet: full card faces, no crops, no pips.
        /// A new game renders sensibly before anyone authors anything for it.
        /// </summary>
        private static DeckRenderConfigApiModel Fallback() => new();
    }
}
*/