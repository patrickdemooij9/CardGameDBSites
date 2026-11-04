using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CardGameDBSites.VideoGenerator.Rendering.Slides;

/// <summary>A full-screen opener that covers the header, so it wipes away to reveal the video.</summary>
public sealed class HookSlide(Image<Rgba32> screen) : ISlide
{
    public void Draw(Image<Rgba32> frame, double centerX)
    {
        var left = (int)Math.Round(centerX - Layout.Width / 2d);
        frame.Mutate(ctx => ctx.DrawImage(screen, new Point(left, 0), 1));
    }

    public void Dispose() => screen.Dispose();
}
