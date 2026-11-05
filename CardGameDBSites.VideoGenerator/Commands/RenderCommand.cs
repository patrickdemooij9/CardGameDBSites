using CardGameDBSites.VideoGenerator.Models;
using CardGameDBSites.VideoGenerator.Output;
using CardGameDBSites.VideoGenerator.Rendering;
using CardGameDBSites.VideoGenerator.Rendering.SetValue;
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

        var json = await File.ReadAllTextAsync(specPath);
        var isSetValue = JsonSerializer.Deserialize<JsonElement>(json).TryGetProperty("format", out var format)
            && format.GetString() == SetValueSpec.FormatName;

        var stopwatch = Stopwatch.StartNew();
        int frames, framesPerSecond;
        if (isSetValue)
        {
            var spec = Deserialize<SetValueSpec>(json, specPath);
            var timing = spec.Timing ?? new SetValueTiming();
            framesPerSecond = timing.FramesPerSecond;
            await using var sink = CreateSink(writeFrames, outputPath, ffmpegPath, framesPerSecond);
            frames = await new SetValueRenderer().Render(spec, timing, sink);
        }
        else
        {
            var spec = Deserialize<RevealVideoSpec>(json, specPath);
            var timing = spec.Timing ?? new RevealTiming();
            framesPerSecond = timing.FramesPerSecond;
            await using var sink = CreateSink(writeFrames, outputPath, ffmpegPath, framesPerSecond);
            frames = await new CardRevealRenderer().Render(spec, timing, sink);
        }

        Console.WriteLine($"Rendered {frames} frames ({frames / (double)framesPerSecond:0.0}s) to {outputPath} in {stopwatch.Elapsed.TotalSeconds:0.0}s");
        return 0;
    }

    private static T Deserialize<T>(string json, string specPath) =>
        JsonSerializer.Deserialize<T>(json, SpecJson.Options) ?? throw new InvalidOperationException($"Could not read {specPath}");

    private static IFrameSink CreateSink(bool writeFrames, string outputPath, string ffmpegPath, int framesPerSecond) => writeFrames
        ? new PngFrameSink(outputPath)
        : new FfmpegFrameSink(ffmpegPath, outputPath, Layout.Width, Layout.Height, framesPerSecond);
}
