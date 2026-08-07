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
    /// Renders shareable tournament infographic slides (1080x1350, TikTok carousel format).
    /// Shared drawing primitives live in <see cref="InfographicRendererBase"/>.
    /// </summary>
    public class TournamentInfographicExport : InfographicRendererBase
    {
        public TournamentInfographicExport(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment)
        {
        }

        // ---- Slide 1: Hook -------------------------------------------------

        public async Task<byte[]> RenderHookSlide(TournamentInfographicData data)
        {
            using var image = NewCanvas();
            RenderBackground(image);
            await RenderLogo(image, 110);

            DrawCenteredText(image, "TOURNAMENT RESULTS", SemiBold.CreateFont(38), Accent, 360, letterSpacing: 8f);

            var nameFont = FitFont(data.TournamentName, ExtraBold, Width - Margin * 2, 260, startSize: 100, minSize: 46);
            var nameOptions = new RichTextOptions(nameFont)
            {
                Origin = new Vector2(Width / 2f, 450),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                WrappingLength = Width - Margin * 2,
                LineSpacing = 1.05f
            };
            image.Mutate(ctx => ctx.DrawText(nameOptions, data.TournamentName, TextColor));

            image.Mutate(ctx => ctx.Fill(Accent, new RectangularPolygon(Width / 2f - 60, 780, 120, 6)));

            DrawCenteredText(image, data.PlayerCount.ToString(), ExtraBold.CreateFont(130), Accent, 815);
            DrawCenteredText(image, "PLAYERS", Bold.CreateFont(48), TextColor, 980, letterSpacing: 10f);
            DrawCenteredText(image, data.DateUtc.ToString("MMMM d, yyyy"), Medium.CreateFont(42), MutedColor, 1055);

            RenderCtaPill(image, "Swipe to see what won", 1175);
            //RenderFooter(image, data.FooterText);

            return await ToPng(image);
        }

        // ---- Slide 2: Top 8 breakdown -------------------------------------

        public async Task<byte[]> RenderTop8Slide(TournamentInfographicData data)
        {
            using var image = NewCanvas();
            RenderBackground(image);
            RenderHeader(image, "TOP 8 BREAKDOWN", data.TournamentName, 78);

            var entries = data.Entries.OrderBy(e => e.Placement).Take(8).ToArray();

            const int rowsTop = 360;
            const int rowsBottom = 1240;
            const int winnerHeight = 240;
            var restRowHeight = entries.Length > 1 ? (rowsBottom - rowsTop - winnerHeight) / (entries.Length - 1) : 0;

            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                var isWinner = i == 0;
                var rowY = isWinner ? rowsTop : rowsTop + winnerHeight + (i - 1) * restRowHeight;
                var rowHeight = isWinner ? winnerHeight : restRowHeight;
                await RenderTop8Row(image, entry, rowY, rowHeight, isWinner);
            }

            //RenderFooter(image, data.FooterText);
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
            var badgeRadius = isWinner ? 46f : 32f;
            var badgeX = Margin + badgeRadius;
            var badgeTextColor = entry.Placement <= 3 ? Ink : TextColor;
            image.Mutate(ctx => ctx.Fill(badgeColor, new EllipsePolygon(badgeX, centerY, badgeRadius)));
            var badgeOptions = new RichTextOptions(ExtraBold.CreateFont(isWinner ? 46f : 32f))
            {
                Origin = new Vector2(badgeX, centerY),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            image.Mutate(ctx => ctx.DrawText(badgeOptions, entry.Placement.ToString(), badgeTextColor));

            var thumbW = isWinner ? 130 : 62;
            var thumbH = isWinner ? 182 : 86;
            var thumbX = (int)(badgeX + badgeRadius + 40);
            var thumbY = (int)(centerY - thumbH / 2f);
            await RenderCardImage(image, entry.ChampionImageUrl, new Point(thumbX, thumbY), new Size(thumbW, thumbH), crop: true);

            var textX = thumbX + thumbW + 36;
            var championName = entry.ChampionName ?? entry.DeckName ?? "Unknown";

            if (isWinner)
            {
                var kickerOptions = new RichTextOptions(Bold.CreateFont(30))
                {
                    Origin = new Vector2(textX, centerY - 70),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };
                image.Mutate(ctx => ctx.DrawText(kickerOptions, "WINNER", Gold));

                var champOptions = new RichTextOptions(ExtraBold.CreateFont(46))
                {
                    Origin = new Vector2(textX, centerY - 6),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    WrappingLength = Width - textX - Margin
                };
                image.Mutate(ctx => ctx.DrawText(champOptions, championName, TextColor));

                var playerOptions = new RichTextOptions(Medium.CreateFont(32))
                {
                    Origin = new Vector2(textX, centerY + 58),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };
                image.Mutate(ctx => ctx.DrawText(playerOptions, entry.PlayerName, MutedColor));
                return;
            }

            var normalChampOptions = new RichTextOptions(Bold.CreateFont(38))
            {
                Origin = new Vector2(textX, centerY - 22),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                WrappingLength = Width - textX - Margin
            };
            image.Mutate(ctx => ctx.DrawText(normalChampOptions, championName, TextColor));

            var normalPlayerOptions = new RichTextOptions(Medium.CreateFont(26))
            {
                Origin = new Vector2(textX, centerY + 24),
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

            RenderAspectBars(image, data.Aspects ?? []);

            //RenderFooter(image, data.FooterText);
            return await ToPng(image);
        }

        // ---- Slide 4: Most played cards -----------------------------------

        public async Task<byte[]> RenderCardsSlide(TournamentInfographicData data)
        {
            using var image = NewCanvas();
            RenderBackground(image);
            RenderHeader(image, "MOST PLAYED CARDS", data.TournamentName, 68);

            await RenderCardGrid(image, data.TopCards ?? []);

            //RenderFooter(image, data.FooterText);
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
