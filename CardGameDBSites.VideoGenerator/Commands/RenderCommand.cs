using CardGameDBSites.VideoGenerator.Models;
using CardGameDBSites.VideoGenerator.Output;
using CardGameDBSites.VideoGenerator.Rendering;
using System.Diagnostics;
using System.Text.Json;

namespace CardGameDBSites.VideoGenerator.Commands;

public static class RenderCommand
{
    public static async Task<int> Run(string[] args)
    {
        var specPath = args[0];
        var outputPath = args[1];
        var writeFrames = args.Contains("--frames");
        var ffmpegPath = CommandArgs.Option(args, "--ffmpeg") ?? "ffmpeg";

        var spec = JsonSerializer.Deserialize<RevealVideoSpec>(await File.ReadAllTextAsync(specPath), SpecJson.Options)
            ?? throw new InvalidOperationException($"Could not read {specPath}");

        var timing = spec.Timing ?? new RevealTiming();
        var stopwatch = Stopwatch.StartNew();

        int frames;
        await using (IFrameSink sink = writeFrames
            ? new PngFrameSink(outputPath)
            : new FfmpegFrameSink(ffmpegPath, outputPath, Layout.Width, Layout.Height, timing.FramesPerSecond))
        {
            frames = await new CardRevealRenderer().Render(spec, timing, sink);
        }

        Console.WriteLine($"Rendered {frames} frames ({frames / (double)timing.FramesPerSecond:0.0}s) to {outputPath} in {stopwatch.Elapsed.TotalSeconds:0.0}s");
        return 0;
    }
}
