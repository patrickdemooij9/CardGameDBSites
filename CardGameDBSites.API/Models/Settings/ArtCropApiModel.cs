namespace CardGameDBSites.API.Models.Settings;

public class ArtCropApiModel
{
    /// <summary>The card-type value this band applies to.</summary>
    public string TypeValue { get; set; } = string.Empty;

    public double Left { get; set; }
    public double Top { get; set; }
    public double Right { get; set; }
    public double Bottom { get; set; }
}