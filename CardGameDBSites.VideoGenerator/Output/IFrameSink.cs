using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CardGameDBSites.VideoGenerator.Output;

public interface IFrameSink : IAsyncDisposable
{
    Task WriteFrame(Image<Rgba32> frame);
}
