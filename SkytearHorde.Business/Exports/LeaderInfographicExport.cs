using Microsoft.AspNetCore.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;
using SixLabors.Fonts;

namespace SkytearHorde.Business.Exports
{
    /// <summary>
    /// Renders the leader showcase carousel (1080x1350): the leader itself, its meta numbers,
    /// the cards it is most often played with, and the aspects its decks pair with.
    /// </summary>
    public class LeaderInfographicExport : InfographicRendererBase
    {
        // 1080x1350 (4:5, the shared default) so TikTok's bottom UI overlay doesn't clip the content.
        public LeaderInfographicExport(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment)
        {
        }

        public static int SlideCount => 4;

        public Task<byte[]> Render(LeaderInfographicData data, int slide) => slide switch
        {
            1 => RenderShowcaseSlide(data),
            2 => RenderStatsSlide(data),
            3 => RenderCardsSlide(data),
            _ => RenderAspectsSlide(data)
        };

        // ---- Slide 1: The leader ------------------------------------------

        private async Task<byte[]> RenderShowcaseSlide(LeaderInfographicData data)
        {
            using var image = NewCanvas();
            RenderBackground(image);
            await RenderLogo(image, 110);

            DrawCenteredText(image, "LEADER SPOTLIGHT", SemiBold.CreateFont(40), Accent, 300, letterSpacing: 8f);

            // Whole card fitted (uncropped) — this slide exists so people can see who the leader is.
            var cardBox = new Size(400, 560);
            await RenderCardImage(image, data.LeaderImageUrl, new Point((Width - cardBox.Width) / 2, 370), cardBox, crop: false);

            var nameFont = FitFont(data.LeaderName, ExtraBold, Width - Margin * 2, 110, startSize: 76, minSize: 40);
            var nameOptions = new RichTextOptions(nameFont)
            {
                Origin = new Vector2(Width / 2f, 960),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                WrappingLength = Width - Margin * 2,
                LineSpacing = 1.05f
            };
            image.Mutate(ctx => ctx.DrawText(nameOptions, data.LeaderName, TextColor));

            if (!string.IsNullOrWhiteSpace(data.SetName))
            {
                DrawCenteredText(image, data.SetName!, Medium.CreateFont(34), MutedColor, 1085);
            }

            RenderCtaPill(image, "Swipe to see the numbers", 1185);
            //RenderFooter(image, data.FooterText);

            return await ToPng(image);
        }

        // ---- Slide 2: Meta statistics --------------------------------------

        private async Task<byte[]> RenderStatsSlide(LeaderInfographicData data)
        {
            using var image = NewCanvas();
            RenderBackground(image);
            RenderHeader(image, "META STATISTICS", data.LeaderName, 72);

            RenderStatBar(image, "WINRATE", data.WinratePercentage, 420);
            RenderStatBar(image, "USAGE RATE", data.UsagePercentage, 610);

            image.Mutate(ctx => ctx.Fill(TrackColor, new RectangularPolygon(Margin, 760, Width - Margin * 2, 2)));

            DrawCenteredText(image, data.DeckCount.ToString("N0"), ExtraBold.CreateFont(130), Accent, 810);
            DrawCenteredText(image, "DECKS", Bold.CreateFont(46), TextColor, 970, letterSpacing: 10f);

            if (!string.IsNullOrWhiteSpace(data.PeriodLabel))
            {
                DrawCenteredText(image, data.PeriodLabel!, Medium.CreateFont(34), MutedColor, 1060);
            }

            //RenderFooter(image, data.FooterText);
            return await ToPng(image);
        }

        /// <summary>A percentage stat: label on the left, value on the right, filled track underneath.</summary>
        private void RenderStatBar(Image<Rgba32> image, string label, double percentage, float labelY)
        {
            var labelOptions = new RichTextOptions(Bold.CreateFont(46))
            {
                Origin = new Vector2(Margin, labelY),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            image.Mutate(ctx => ctx.DrawText(labelOptions, label, TextColor));

            var valueOptions = new RichTextOptions(ExtraBold.CreateFont(64))
            {
                Origin = new Vector2(Width - Margin, labelY),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            image.Mutate(ctx => ctx.DrawText(valueOptions, FormatPercentage(percentage), Accent));

            var trackWidth = Width - Margin * 2;
            var trackY = labelY + 48;
            const float trackH = 44f;
            RenderRoundedBar(image, Margin, trackY, trackWidth, trackH, TrackColor);
            var fillWidth = trackWidth * (float)Math.Clamp(percentage, 0, 100) / 100f;
            if (fillWidth > 0) RenderRoundedBar(image, Margin, trackY, fillWidth, trackH, Accent);
        }

        /// <summary>Rounds to a whole percent, but keeps a non-zero sliver visible as "&lt;1%" rather than "0%".</summary>
        private static string FormatPercentage(double value)
        {
            if (value > 0 && value < 1) return "<1%";
            return $"{Math.Round(value)}%";
        }

        // ---- Slide 3: Cards most played with -------------------------------

        private async Task<byte[]> RenderCardsSlide(LeaderInfographicData data)
        {
            using var image = NewCanvas();
            RenderBackground(image);
            RenderHeader(image, "MOST PLAYED WITH", data.LeaderName, 68);

            await RenderCardGrid(image, data.TopCards ?? []);

            //RenderFooter(image, data.FooterText);
            return await ToPng(image);
        }

        // ---- Slide 4: Aspects paired with ---------------------------------

        private async Task<byte[]> RenderAspectsSlide(LeaderInfographicData data)
        {
            using var image = NewCanvas();
            RenderBackground(image);
            RenderHeader(image, "ASPECTS PAIRED WITH", data.LeaderName, 62);

            RenderAspectBars(image, data.Aspects ?? []);

            //RenderFooter(image, data.FooterText);
            return await ToPng(image);
        }
    }

    public class LeaderInfographicData
    {
        public required string LeaderName { get; set; }
        public string? LeaderImageUrl { get; set; }

        /// <summary>Shown under the leader name on the showcase slide.</summary>
        public string? SetName { get; set; }

        /// <summary>Which period the numbers cover; shown under the deck count on the stats slide.</summary>
        public string? PeriodLabel { get; set; }

        public double WinratePercentage { get; set; }
        public double UsagePercentage { get; set; }
        public int DeckCount { get; set; }

        public IReadOnlyList<InfographicCard>? TopCards { get; set; }
        public IReadOnlyList<InfographicAspect>? Aspects { get; set; }
        public string? FooterText { get; set; }
    }
}
