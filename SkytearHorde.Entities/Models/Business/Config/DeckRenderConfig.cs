namespace SkytearHorde.Entities.Models.Business.Config;

public class DeckRenderConfig
{
    public double CardAspect { get; set; } = 0.7;

    public string[] LandscapeTypes { get; set; } = [];

    public string[] BackImageTypes { get; set; } = [];

    public string? TypeAttribute { get; set; }
    public string? CostAttribute { get; set; }
    public string? AspectAttribute { get; set; }
    public Dictionary<string, string> AspectColors { get; set; } = [];
    /// </summary>
    public ArtCropModel[] ArtCrops { get; set; } = [];

    /// <summary>Small label above the deck name. Falls back to a derived label when null.</summary>
    public string? Eyebrow { get; set; }

    /// <summary>Card count at or above which art crops are used, if crops exist.</summary>
    public int TierBMin { get; set; } = 46;

    /// <summary>Card count at or above which the typeset list is forced.</summary>
    public int TierCMin { get; set; } = 141;

    /// <summary>Output is never shorter than width * this. Stops small decks becoming banners.</summary>
    public double MinCanvasRatio { get; set; } = 1.0;
}