using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CardGameDBSites.VideoGenerator.Rendering.Grid;

public sealed record GridCell(Image<Rgba32> Image, Point Target) : IDisposable
{
    public void Dispose() => Image.Dispose();
}
