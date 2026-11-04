using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CardGameDBSites.VideoGenerator.Rendering.Slides;

public interface ISlide : IDisposable
{
    void Draw(Image<Rgba32> frame, double centerX);
}
