using CardGameDBSites.VideoGenerator.Models;
using CardGameDBSites.VideoGenerator.Rendering.Slides;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CardGameDBSites.VideoGenerator.Rendering;

/// <param name="startCentred">The first slide is already on screen at frame 0 instead of sliding in.</param>
public sealed class Carousel(IReadOnlyList<CarouselEntry> entries, RevealTiming timing, bool startCentred) : IDisposable
{
    // At least half a screen plus half a label, so only one slide is ever centred.
    private const double SlideSpacing = Layout.Width / 2d + Layout.LabelWidth / 2d + Layout.ShadowPadding;

    private double SkippedSeconds => startCentred ? timing.IntroSeconds + timing.TransitionSeconds : 0;

    /// <summary>Seconds from the start of the video until the last slide has left the screen.</summary>
    public double EndSeconds =>
        timing.IntroSeconds + entries.Sum(e => e.HoldSeconds) + (entries.Count + 1) * timing.TransitionSeconds - SkippedSeconds;

    public void Draw(Image<Rgba32> frame, double seconds)
    {
        var scroll = ScrollPosition(seconds);
        for (var i = 0; i < entries.Count; i++)
        {
            var offset = i - scroll;
            if (Math.Abs(offset) < 1) entries[i].Slide.Draw(frame, Layout.Width / 2d + offset * SlideSpacing);
        }
    }

    /// <summary>How much of the screen the group owns right now (0–1); neighbouring groups crossfade.</summary>
    public double GroupWeight(int groupIndex, double seconds)
    {
        var scroll = ScrollPosition(seconds);
        var weight = 0d;
        for (var i = 0; i < entries.Count; i++)
        {
            if (entries[i].GroupIndex == groupIndex) weight += Math.Max(0, 1 - Math.Abs(i - scroll));
        }
        return Math.Min(weight, 1);
    }

    /// <summary>Index of the centred slide: -1 before the first slides in, Count after the last slides out.</summary>
    private double ScrollPosition(double seconds)
    {
        var remaining = seconds + SkippedSeconds - timing.IntroSeconds;
        if (remaining < 0) return -1;

        for (var i = 0; i <= entries.Count; i++)
        {
            if (remaining < timing.TransitionSeconds) return i - 1 + Easing.InOutCubic(remaining / timing.TransitionSeconds);
            remaining -= timing.TransitionSeconds;

            if (i == entries.Count) break;
            if (remaining < entries[i].HoldSeconds) return i;
            remaining -= entries[i].HoldSeconds;
        }
        return entries.Count;
    }

    public void Dispose()
    {
        foreach (var entry in entries) entry.Slide.Dispose();
    }
}
