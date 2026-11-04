using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;

namespace CardGameDBSites.VideoGenerator.Output;

public sealed class FfmpegFrameSink : IFrameSink
{
    private readonly Process _process;
    private readonly Stream _input;
    private byte[] _buffer = [];

    public FfmpegFrameSink(string ffmpegPath, string outputPath, int width, int height, int framesPerSecond)
    {
        var startInfo = new ProcessStartInfo(ffmpegPath)
        {
            RedirectStandardInput = true,
            UseShellExecute = false
        };
        string[] args =
        [
            "-y", "-loglevel", "error",
            "-f", "rawvideo", "-pix_fmt", "rgba", "-s", $"{width}x{height}", "-r", framesPerSecond.ToString(), "-i", "-",
            "-c:v", "libx264", "-preset", "medium", "-crf", "18", "-pix_fmt", "yuv420p", "-movflags", "+faststart",
            outputPath
        ];
        foreach (var arg in args) startInfo.ArgumentList.Add(arg);

        _process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Could not start {ffmpegPath}");
        _input = _process.StandardInput.BaseStream;
    }

    public async Task WriteFrame(Image<Rgba32> frame)
    {
        var size = frame.Width * frame.Height * 4;
        if (_buffer.Length != size) _buffer = new byte[size];

        frame.CopyPixelDataTo(_buffer);
        await _input.WriteAsync(_buffer);
    }

    public async ValueTask DisposeAsync()
    {
        await _input.DisposeAsync();
        await _process.WaitForExitAsync();
        var exitCode = _process.ExitCode;
        _process.Dispose();

        if (exitCode != 0) throw new InvalidOperationException($"ffmpeg exited with code {exitCode}");
    }
}
