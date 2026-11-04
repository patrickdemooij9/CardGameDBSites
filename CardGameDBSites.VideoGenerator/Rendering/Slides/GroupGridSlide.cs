using CardGameDBSites.VideoGenerator.Rendering.Grid;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CardGameDBSites.VideoGenerator.Rendering.Slides;

public sealed class GroupGridSlide(IReadOnlyList<GridCell> cells) : ISlide
{
    public void Draw(Image<Rgba32> frame, double centerX)
    {
        var offsetX = (int)Math.Round(centerX - Layout.Width / 2d);
        frame.Mutate(ctx =>
        {
            foreach (var cell in cells) ctx.DrawImage(cell.Image, new Point(cell.Target.X + offsetX, cell.Target.Y), 1);
        });
    }

    public void Dispose()
    {
        foreach (var cell in cells) cell.Dispose();
    }
}
