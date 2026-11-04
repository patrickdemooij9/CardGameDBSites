using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CardGameDBSites.VideoGenerator.Output;

public sealed class PngFrameSink(string directory, int everyNthFrame = 1) : IFrameSink
{
    private int _frameIndex;

    public async Task WriteFrame(Image<Rgba32> frame)
    {
        if (_frameIndex % everyNthFrame == 0)
        {
            Directory.CreateDirectory(directory);
            await frame.SaveAsPngAsync(Path.Combine(directory, $"frame_{_frameIndex:D5}.png"));
        }
        _frameIndex++;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
