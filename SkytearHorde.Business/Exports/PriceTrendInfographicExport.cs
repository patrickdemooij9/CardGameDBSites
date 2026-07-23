using Microsoft.AspNetCore.Hosting;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Globalization;
using System.Numerics;
using Image = SixLabors.ImageSharp.Image;

namespace SkytearHorde.Business.Exports
{
    /// <summary>
    /// Renders the weekly price-trend carousel (1080x1920): a hook, then the top rising
    /// and top dropping cards of the week.
    /// </summary>
    public class PriceTrendInfographicExport : InfographicRendererBase
    {
        private static readonly Color UpColor = Color.ParseHex("#46D17F");
        private static readonly Color DownColor = Color.ParseHex("#FF5C5C");

        public PriceTrendInfographicExport(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment)
        {
        }

        public static int SlideCount => 3;

        public Task<byte[]> Render(PriceTrendInfographicData data, int slide) => slide switch
        {
            1 => RenderHookSlide(data),
            2 => RenderMoversSlide(data, "TOP RISING CARDS", data.Risers, rising: true),
            _ => RenderMoversSlide(data, "TOP DROPPING CARDS", data.Fallers, rising: false)
        };

        // ---- Slide 1: Hook -------------------------------------------------

        private async Task<byte[]> RenderHookSlide(PriceTrendInfographicData data)
        {
            using var image = NewCanvas();
            RenderBackground(image);
            await RenderLogo(image, 150);

            DrawCenteredText(image, "WEEKLY PRICE WATCH", SemiBold.CreateFont(40), Accent, 560, letterSpacing: 8f);

            var titleFont = FitFont("This week's biggest movers", ExtraBold, Width - Margin * 2, 480, startSize: 108, minSize: 60);
            var titleOptions = new RichTextOptions(titleFont)
            {
                Origin = new Vector2(Width / 2f, 720),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                WrappingLength = Width - Margin * 2,
                LineSpacing = 1.05f
            };
            image.Mutate(ctx => ctx.DrawText(titleOptions, "This week's biggest movers", TextColor));

            if (!string.IsNullOrWhiteSpace(data.DateRangeLabel))
            {
                DrawCenteredText(image, data.DateRangeLabel!, Medium.CreateFont(44), MutedColor, 1160);
            }

            RenderCtaPill(image, "Swipe to see", 1700);
            RenderFooter(image, data.FooterText);

            return await ToPng(image);
        }

        // ---- Movers slides -------------------------------------------------

        private async Task<byte[]> RenderMoversSlide(PriceTrendInfographicData data, string heading, IReadOnlyList<PriceTrendEntry> entries, bool rising)
        {
            using var image = NewCanvas();
            RenderBackground(image);
            RenderHeader(image, heading, data.DateRangeLabel, 66);

            const int rowsTop = 400;
            const int rowsBottom = 1780;
            var count = Math.Max(entries.Count, 1);
            var rowHeight = (rowsBottom - rowsTop) / count;

            for (var i = 0; i < entries.Count; i++)
            {
                await RenderMoverRow(image, entries[i], rowsTop + i * rowHeight, rowHeight, rising);
            }

            RenderFooter(image, data.FooterText);
            return await ToPng(image);
        }

        private async Task RenderMoverRow(Image<Rgba32> image, PriceTrendEntry entry, int rowY, int rowHeight, bool rising)
        {
            var centerY = rowY + rowHeight / 2f;
            var changeColor = rising ? UpColor : DownColor;

            // Rank
            var rankOptions = new RichTextOptions(ExtraBold.CreateFont(46))
            {
                Origin = new Vector2(Margin, centerY),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            image.Mutate(ctx => ctx.DrawText(rankOptions, entry.Rank.ToString(), MutedColor));

            // Card thumbnail
            var thumbW = 116;
            var thumbH = 162;
            var thumbX = Margin + 70;
            var thumbY = (int)(centerY - thumbH / 2f);
            await RenderCardImage(image, entry.ImageUrl, new Point(thumbX, thumbY), new Size(thumbW, thumbH), crop: true);

            // Name + old -> new price
            var textX = thumbX + thumbW + 34;
            var nameOptions = new RichTextOptions(Bold.CreateFont(46))
            {
                Origin = new Vector2(textX, centerY - 26),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                WrappingLength = Width - textX - 260
            };
            image.Mutate(ctx => ctx.DrawText(nameOptions, entry.Name, TextColor));

            // "$old  ▸  $new" — the arrow is drawn as a triangle since Barlow has no → glyph.
            var priceFont = Medium.CreateFont(32);
            var priceY = centerY + 34;
            var oldStr = Money(entry.OldPrice);
            var newStr = Money(entry.NewPrice);

            image.Mutate(ctx => ctx.DrawText(new RichTextOptions(priceFont)
            {
                Origin = new Vector2(textX, priceY),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            }, oldStr, MutedColor));

            var oldWidth = TextMeasurer.MeasureSize(oldStr, new TextOptions(priceFont)).Width;
            var arrowX = textX + oldWidth + 18;
            var arrowSize = 18f;
            var arrow = new Polygon(new LinearLineSegment(
                new PointF(arrowX, priceY - arrowSize / 2f),
                new PointF(arrowX + arrowSize, priceY),
                new PointF(arrowX, priceY + arrowSize / 2f)));
            image.Mutate(ctx => ctx.Fill(MutedColor, arrow));

            image.Mutate(ctx => ctx.DrawText(new RichTextOptions(priceFont)
            {
                Origin = new Vector2(arrowX + arrowSize + 18, priceY),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            }, newStr, MutedColor));

            // Change % (right aligned, coloured)
            var pct = entry.OldPrice > 0 ? (entry.NewPrice - entry.OldPrice) / entry.OldPrice * 100.0 : 0.0;
            var pctText = $"{(pct >= 0 ? "+" : "")}{Math.Round(pct)}%";
            var pctOptions = new RichTextOptions(ExtraBold.CreateFont(56))
            {
                Origin = new Vector2(Width - Margin, centerY),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            image.Mutate(ctx => ctx.DrawText(pctOptions, pctText, changeColor));

            // Separator
            var sepY = rowY + rowHeight - 2;
            image.Mutate(ctx => ctx.Fill(TrackColor, new RectangularPolygon(Margin, sepY, Width - Margin * 2, 2)));
        }

        private static string Money(double value) => "$" + value.ToString("0.00", CultureInfo.InvariantCulture);
    }

    public class PriceTrendInfographicData
    {
        public string? DateRangeLabel { get; set; }
        public required IReadOnlyList<PriceTrendEntry> Risers { get; set; }
        public required IReadOnlyList<PriceTrendEntry> Fallers { get; set; }
        public string? FooterText { get; set; }
    }

    public class PriceTrendEntry
    {
        public required int Rank { get; set; }
        public required string Name { get; set; }
        public string? ImageUrl { get; set; }
        public required double OldPrice { get; set; }
        public required double NewPrice { get; set; }
    }
}
