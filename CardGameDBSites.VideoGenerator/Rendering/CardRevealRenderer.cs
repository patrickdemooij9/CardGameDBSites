using CardGameDBSites.VideoGenerator.Models;
using CardGameDBSites.VideoGenerator.Output;
using CardGameDBSites.VideoGenerator.Rendering.Finale;
using CardGameDBSites.VideoGenerator.Rendering.Slides;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CardGameDBSites.VideoGenerator.Rendering;

public class CardRevealRenderer
{
    private const double SubheadingFadeSeconds = 0.4;

    private readonly Fonts _fonts = new();

    /// <returns>The number of frames written.</returns>
    public async Task<int> Render(RevealVideoSpec spec, RevealTiming timing, IFrameSink sink)
    {
        using var imageLoader = new CardImageLoader();
        var groups = spec.AllGroups();

        using var background = RenderBackground(spec);
        var entries = await new SlideFactory(_fonts, imageLoader).Build(spec.Hook, groups, timing);
        using var carousel = new Carousel(entries, timing, startCentred: spec.Hook is not null);
        using var finale = spec.Finale is null ? null : await DeckFinale.Build(spec.FinaleCards(), timing, _fonts, imageLoader);

        using var subtitle = RenderSubheading(spec.Subtitle, Palette.Muted);
        var groupHeadings = groups.Select(g => RenderSubheading(g.Name?.ToUpperInvariant(), Palette.Accent)).ToList();
        using var finaleHeading = RenderSubheading(spec.Finale?.Title?.ToUpperInvariant(), Palette.Accent);

        var totalSeconds = carousel.EndSeconds + (finale?.DurationSeconds ?? timing.OutroSeconds);
        var totalFrames = (int)Math.Ceiling(totalSeconds * timing.FramesPerSecond);

        try
        {
            for (var frameIndex = 0; frameIndex < totalFrames; frameIndex++)
            {
                var seconds = frameIndex / (double)timing.FramesPerSecond;
                var finaleSeconds = seconds - carousel.EndSeconds;
                using var frame = background.Clone();

                var finaleWeight = finale is null ? 0 : Math.Clamp(finaleSeconds / SubheadingFadeSeconds, 0, 1);
                var headingWeight = finaleWeight;
                for (var g = 0; g < groupHeadings.Count; g++)
                {
                    if (groupHeadings[g] is null) continue;
                    var weight = carousel.GroupWeight(g, seconds);
                    headingWeight += weight;
                    DrawFaded(frame, groupHeadings[g], Math.Clamp((weight - 0.5) * 4 + 0.5, 0, 1));
                }
                DrawFaded(frame, finaleHeading, finaleWeight);
                DrawFaded(frame, subtitle, 1 - headingWeight);

                carousel.Draw(frame, seconds);
                if (finale is not null && finaleSeconds > 0) finale.Draw(frame, finaleSeconds);

                await sink.WriteFrame(frame);
            }
        }
        finally
        {
            groupHeadings.ForEach(h => h?.Dispose());
        }

        return totalFrames;
    }

    private static void DrawFaded(Image<Rgba32> frame, Image<Rgba32>? layer, double opacity)
    {
        if (layer is null || opacity <= 0.01) return;
        frame.Mutate(ctx => ctx.DrawImage(layer, new Point(0, Layout.SubtitleY), (float)Math.Min(opacity, 1)));
    }

    private Image<Rgba32>? RenderSubheading(string? text, Color color)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        var image = new Image<Rgba32>(Layout.Width, 80);
        var font = TextDrawing.FitFont(text, _fonts.SemiBold, Layout.Width - 80, startSize: 46, minSize: 32);
        TextDrawing.DrawCentered(image, text, font, color, 0);
        return image;
    }

    private Image<Rgba32> RenderBackground(RevealVideoSpec spec)
    {
        var image = Backdrop.Render();

        if (!string.IsNullOrWhiteSpace(spec.Title))
        {
            var titleFont = TextDrawing.FitFont(spec.Title, _fonts.ExtraBold, Layout.Width - 80, startSize: 84, minSize: 56);
            TextDrawing.DrawCentered(image, spec.Title, titleFont, Palette.Text, Layout.TitleY);
            image.Mutate(ctx => ctx.Fill(Palette.Accent, new RectangularPolygon(Layout.Width / 2f - 90, 262, 180, 6)));
        }
        if (!string.IsNullOrWhiteSpace(spec.Footer))
        {
            TextDrawing.DrawCentered(image, spec.Footer, _fonts.SemiBold.CreateFont(36), Palette.Muted, Layout.FooterY);
        }

        return image;
    }
}
