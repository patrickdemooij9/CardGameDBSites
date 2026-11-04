using SixLabors.ImageSharp;

namespace CardGameDBSites.VideoGenerator.Rendering;

// Mirrors InfographicRendererBase so videos match the gallery posts.
public static class Palette
{
    public static readonly Color BgTop = Color.ParseHex("#0B1020");
    public static readonly Color BgBottom = Color.ParseHex("#1A2244");
    public static readonly Color Accent = Color.ParseHex("#FFC94A");
    public static readonly Color Text = Color.White;
    public static readonly Color Muted = Color.ParseHex("#9AA3C0");
    public static readonly Color Ink = Color.ParseHex("#20263F");
    public static readonly Color Chip = Color.ParseHex("#2A3358");
}
