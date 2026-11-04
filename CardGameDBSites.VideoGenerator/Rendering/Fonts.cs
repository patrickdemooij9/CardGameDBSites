using SixLabors.Fonts;

namespace CardGameDBSites.VideoGenerator.Rendering;

public class Fonts
{
    public FontFamily ExtraBold { get; } = Load("Barlow-ExtraBold.ttf");
    public FontFamily Bold { get; } = Load("Barlow-Bold.ttf");
    public FontFamily SemiBold { get; } = Load("Barlow-SemiBold.ttf");
    public FontFamily Medium { get; } = Load("Barlow-Medium.ttf");

    private static FontFamily Load(string fileName) =>
        new FontCollection().Add(Path.Combine(AppContext.BaseDirectory, "Assets", "fonts", fileName));
}
