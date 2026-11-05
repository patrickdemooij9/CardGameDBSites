using CardGameDBSites.VideoGenerator.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CardGameDBSites.VideoGenerator.Rendering.SetValue;

/// <param name="StartSeconds">When the card becomes the featured card and its price starts counting.</param>
/// <param name="FeaturedSeconds">How long it stays featured before flying into the binder.</param>
public sealed record SetCard(
    PricedCard Card,
    int Rank,
    Image<Rgba32> Featured,
    Image<Rgba32> Slot,
    Rectangle SlotCell,
    double StartSeconds,
    double FeaturedSeconds,
    double FlySeconds) : IDisposable
{
    public double FlyStartSeconds => StartSeconds + FeaturedSeconds;
    public double LandSeconds => FlyStartSeconds + FlySeconds;

    public Rectangle FeaturedRect => Centered(SetValueLayout.Feature, Featured.Size);
    public Rectangle SlotRect => Centered(SlotCell, Slot.Size);

    private static Rectangle Centered(Rectangle box, Size size) =>
        new(box.X + (box.Width - size.Width) / 2, box.Y + (box.Height - size.Height) / 2, size.Width, size.Height);

    public void Dispose()
    {
        Featured.Dispose();
        Slot.Dispose();
    }
}
