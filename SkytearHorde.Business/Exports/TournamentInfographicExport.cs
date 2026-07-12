using Microsoft.AspNetCore.Hosting;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;
using Image = SixLabors.ImageSharp.Image;

namespace SkytearHorde.Business.Exports
{
    /// <summary>
    /// Renders shareable tournament infographic slides (1080x1920, TikTok carousel format).
    /// Shared drawing primitives live in <see cref="InfographicRendererBase"/>.
    /// </summary>
    public class TournamentInfographicExport : InfographicRendererBase
    {
        private static readonly IReadOnlyDictionary<string, Color> AspectColors = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
        {
            ["Vigilance"] = Color.ParseHex("#3B7BBF"),
            ["Command"] = Color.ParseHex("#3E9F4E"),
            ["Aggression"] = Color.ParseHex("#D2232A"),
            ["Cunning"] = Color.ParseHex("#F4B223"),
            ["Heroism"] = Color.ParseHex("#E8E4D4"),
            ["Villainy"] = Color.ParseHex("#4B4453")
        };

        public TournamentInfographicExport(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment)
        {
        }

        // ---- Slide 1: Hook -------------------------------------------------

        public async Task<byte[]> RenderHookSlide(TournamentInfographicData data)
        {
            using var image = NewCanvas();
            RenderBackground(image);
            await RenderLogo(image, 150);

            DrawCenteredText(image, "TOURNAMENT RESULTS", SemiBold.CreateFont(38), Accent, 520, letterSpacing: 8f);

            var nameFont = FitFont(data.TournamentName, ExtraBold, Width - Margin * 2, 620, startSize: 120, minSize: 54);
            var nameOptions = new RichTextOptions(nameFont)
            {
                Origin = new Vector2(Width / 2f, 620),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                WrappingLength = Width - Margin * 2,
                LineSpacing = 1.05f
            };
            image.Mutate(ctx => ctx.DrawText(nameOptions, data.TournamentName, TextColor));

            image.Mutate(ctx => ctx.Fill(Accent, new RectangularPolygon(Width / 2f - 60, 1180, 120, 6)));

            DrawCenteredText(image, data.PlayerCount.ToString(), ExtraBold.CreateFont(150), Accent, 1240);
            DrawCenteredText(image, "PLAYERS", Bold.CreateFont(52), TextColor, 1420, letterSpacing: 10f);
            DrawCenteredText(image, data.DateUtc.ToString("MMMM d, yyyy"), Medium.CreateFont(44), MutedColor, 1510);

            RenderCtaPill(image, "Swipe to see what won", 1690);
            RenderFooter(image, data.FooterText);

            return await ToPng(image);
        }

        // ---- Slide 2: Top 8 breakdown -------------------------------------

        public async Task<byte[]> RenderTop8Slide(TournamentInfographicData data)
        {
            using var image = NewCanvas();
            RenderBackground(image);
            RenderHeader(image, "TOP 8 BREAKDOWN", data.TournamentName, 78);

            var entries = data.Entries.OrderBy(e => e.Placement).Take(8).ToArray();

            const int rowsTop = 380;
            const int rowsBottom = 1780;
            const int winnerHeight = 320;
            var restRowHeight = entries.Length > 1 ? (rowsBottom - rowsTop - winnerHeight) / (entries.Length - 1) : 0;

            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                var isWinner = i == 0;
                var rowY = isWinner ? rowsTop : rowsTop + winnerHeight + (i - 1) * restRowHeight;
                var rowHeight = isWinner ? winnerHeight : restRowHeight;
                await RenderTop8Row(image, entry, rowY, rowHeight, isWinner);
            }

            RenderFooter(image, data.FooterText);
            return await ToPng(image);
        }

        private async Task RenderTop8Row(Image<Rgba32> image, TournamentInfographicEntry entry, int rowY, int rowHeight, bool isWinner)
        {
            var centerY = rowY + rowHeight / 2f;

            if (isWinner)
            {
                // Faint gold spotlight panel behind the winner
                image.Mutate(ctx => ctx.Fill(Gold.WithAlpha(0.10f), new RectangularPolygon(Margin - 20, rowY + 10, Width - (Margin - 20) * 2, rowHeight - 20)));
                image.Mutate(ctx => ctx.Fill(Gold, new RectangularPolygon(Margin - 20, rowY + 10, 8, rowHeight - 20)));
            }

            var badgeColor = entry.Placement switch
            {
                1 => Gold,
                2 => Silver,
                3 => Bronze,
                _ => ChipColor
            };
            var badgeRadius = isWinner ? 62f : 46f;
            var badgeX = Margin + badgeRadius;
            var badgeTextColor = entry.Placement <= 3 ? Ink : TextColor;
            image.Mutate(ctx => ctx.Fill(badgeColor, new EllipsePolygon(badgeX, centerY, badgeRadius)));
            var badgeOptions = new RichTextOptions(ExtraBold.CreateFont(isWinner ? 62f : 48f))
            {
                Origin = new Vector2(badgeX, centerY),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            image.Mutate(ctx => ctx.DrawText(badgeOptions, entry.Placement.ToString(), badgeTextColor));

            var thumbW = isWinner ? 160 : 116;
            var thumbH = isWinner ? 224 : 162;
            var thumbX = (int)(badgeX + badgeRadius + 40);
            var thumbY = (int)(centerY - thumbH / 2f);
            await RenderCardImage(image, entry.ChampionImageUrl, new Point(thumbX, thumbY), new Size(thumbW, thumbH), crop: true);

            var textX = thumbX + thumbW + 36;
            var championName = entry.ChampionName ?? entry.DeckName ?? "Unknown";

            if (isWinner)
            {
                var kickerOptions = new RichTextOptions(Bold.CreateFont(34))
                {
                    Origin = new Vector2(textX, centerY - 78),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };
                image.Mutate(ctx => ctx.DrawText(kickerOptions, "WINNER", Gold));

                var champOptions = new RichTextOptions(ExtraBold.CreateFont(64))
                {
                    Origin = new Vector2(textX, centerY - 6),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    WrappingLength = Width - textX - Margin
                };
                image.Mutate(ctx => ctx.DrawText(champOptions, championName, TextColor));

                var playerOptions = new RichTextOptions(Medium.CreateFont(40))
                {
                    Origin = new Vector2(textX, centerY + 66),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };
                image.Mutate(ctx => ctx.DrawText(playerOptions, entry.PlayerName, MutedColor));
                return;
            }

            var normalChampOptions = new RichTextOptions(Bold.CreateFont(50))
            {
                Origin = new Vector2(textX, centerY - 30),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                WrappingLength = Width - textX - Margin
            };
            image.Mutate(ctx => ctx.DrawText(normalChampOptions, championName, TextColor));

            var normalPlayerOptions = new RichTextOptions(Medium.CreateFont(34))
            {
                Origin = new Vector2(textX, centerY + 32),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            image.Mutate(ctx => ctx.DrawText(normalPlayerOptions, entry.PlayerName, MutedColor));

            var sepY = rowY + rowHeight - 2;
            image.Mutate(ctx => ctx.Fill(TrackColor, new RectangularPolygon(Margin, sepY, Width - Margin * 2, 2)));
        }

        // ---- Slide 3: Aspect breakdown ------------------------------------

        public async Task<byte[]> RenderAspectsSlide(TournamentInfographicData data)
        {
            using var image = NewCanvas();
            RenderBackground(image);
            RenderHeader(image, "ASPECT BREAKDOWN", data.TournamentName, 72);

            var aspects = data.Aspects ?? [];
            if (aspects.Count > 0)
            {
                const int barsTop = 440;
                const int barsBottom = 1770;
                var slot = (barsBottom - barsTop) / aspects.Count;
                var trackWidth = Width - Margin * 2;

                for (var i = 0; i < aspects.Count; i++)
                {
                    var aspect = aspects[i];
                    var color = AspectColors.GetValueOrDefault(aspect.Name, MutedColor);
                    var blockY = barsTop + i * slot;

                    var labelY = blockY + 26;
                    var nameOptions = new RichTextOptions(Bold.CreateFont(46))
                    {
                        Origin = new Vector2(Margin, labelY),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    image.Mutate(ctx => ctx.DrawText(nameOptions, aspect.Name, TextColor));

                    var pctOptions = new RichTextOptions(ExtraBold.CreateFont(46))
                    {
                        Origin = new Vector2(Width - Margin, labelY),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    image.Mutate(ctx => ctx.DrawText(pctOptions, $"{aspect.Percentage}%", color));

                    var trackY = blockY + 68;
                    var trackH = 44f;
                    RenderRoundedBar(image, Margin, trackY, trackWidth, trackH, TrackColor);
                    var fillWidth = trackWidth * Math.Clamp(aspect.Percentage, 0, 100) / 100f;
                    if (fillWidth > 0) RenderRoundedBar(image, Margin, trackY, fillWidth, trackH, color);
                }
            }

            RenderFooter(image, data.FooterText);
            return await ToPng(image);
        }

        // ---- Slide 4: Most played cards -----------------------------------

        public async Task<byte[]> RenderCardsSlide(TournamentInfographicData data)
        {
            using var image = NewCanvas();
            RenderBackground(image);
            RenderHeader(image, "MOST PLAYED CARDS", data.TournamentName, 68);

            var cards = (data.TopCards ?? []).Take(9).ToArray();

            const int cols = 3;
            const int rows = 3;
            const int gap = 34;
            const int gridTop = 420;
            const int gridBottom = 1770;
            const float cardRatio = 0.716f; // width / height

            var areaWidth = Width - Margin * 2;
            var cellW = (areaWidth - gap * (cols - 1)) / cols;
            var cellH = (gridBottom - gridTop - gap * (rows - 1)) / rows;
            var cardH = cellH - 54; // reserve space for the percentage label
            var cardW = cardH * cardRatio;

            for (var i = 0; i < cards.Length; i++)
            {
                var card = cards[i];
                var col = i % cols;
                var row = i / cols;
                var cellX = Margin + col * (cellW + gap);
                var cellY = gridTop + row * (cellH + gap);

                var cardX = (int)(cellX + (cellW - cardW) / 2f);
                await RenderCardImage(image, card.ImageUrl, new Point(cardX, cellY), new Size((int)cardW, (int)cardH), crop: true);

                var pctOptions = new RichTextOptions(ExtraBold.CreateFont(40))
                {
                    Origin = new Vector2(cellX + cellW / 2f, cellY + cardH + 30),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                image.Mutate(ctx => ctx.DrawText(pctOptions, $"{card.Percentage}%", Accent));
            }

            RenderFooter(image, data.FooterText);
            return await ToPng(image);
        }
    }

    public class TournamentInfographicData
    {
        public required string TournamentName { get; set; }
        public required DateTime DateUtc { get; set; }
        public required int PlayerCount { get; set; }
        public required IReadOnlyList<TournamentInfographicEntry> Entries { get; set; }
        public IReadOnlyList<InfographicAspect>? Aspects { get; set; }
        public IReadOnlyList<InfographicCard>? TopCards { get; set; }
        public string? FooterText { get; set; }
    }

    public class TournamentInfographicEntry
    {
        public required int Placement { get; set; }
        public required string PlayerName { get; set; }
        public string? DeckName { get; set; }
        public string? ChampionName { get; set; }
        public string? ChampionImageUrl { get; set; }
    }

    public class InfographicAspect
    {
        public required string Name { get; set; }
        public required int Percentage { get; set; }
    }

    public class InfographicCard
    {
        public required string Name { get; set; }
        public string? ImageUrl { get; set; }
        public required int Percentage { get; set; }
    }
}
