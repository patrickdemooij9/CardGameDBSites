using CardGameDBSites.VideoGenerator.Models;
using CardGameDBSites.VideoGenerator.Output;
using CardGameDBSites.VideoGenerator.Rendering.Grid;
using CardGameDBSites.VideoGenerator.Rendering.Slides;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Globalization;
using System.Numerics;

namespace CardGameDBSites.VideoGenerator.Rendering.SetValue;

public class SetValueRenderer
{
    private const double SlowCardSeconds = 0.3;
    private const double FeaturedRefreshSeconds = 0.125;
    private const double PriceRollSeconds = 0.25;
    private const double PopSeconds = 0.2;
    private const int ImageDownloads = 8;

    private static readonly CultureInfo Money = CultureInfo.GetCultureInfo("en-US");
    private static readonly Color Pink = Color.ParseHex("#FF6FB5");
    private static readonly decimal[] PriceTiers = [0.25m, 0.5m, 1, 2, 5, 10, 20, 50, 100, 250, 500, 1000];

    private readonly Fonts _fonts = new();
    private readonly Dictionary<int, Image<Rgba32>> _infoPanels = [];
    private readonly Dictionary<Size, Image<Rgba32>> _shadows = [];

    /// <returns>The number of frames written.</returns>
    public async Task<int> Render(SetValueSpec spec, SetValueTiming timing, IFrameSink sink)
    {
        using var imageLoader = new CardImageLoader(cache: false);
        var cardsStart = spec.Hook is null ? 0.3 : timing.HookSeconds + timing.WipeSeconds / 2;

        var cards = await LoadCards(spec.Cards, timing, cardsStart, imageLoader);
        using var hook = spec.Hook is null ? null : await new SlideFactory(_fonts, imageLoader).BuildHook(spec.Hook);
        using var background = await RenderBackground(spec, imageLoader);
        using var binder = RenderEmptyBinder(cards);

        var finaleStart = cards.Count == 0 ? cardsStart : cards[^1].LandSeconds;
        var totalFrames = (int)Math.Ceiling((finaleStart + timing.FinaleSeconds) * timing.FramesPerSecond);
        var landed = 0;

        try
        {
            for (var frameIndex = 0; frameIndex < totalFrames; frameIndex++)
            {
                var seconds = frameIndex / (double)timing.FramesPerSecond;
                for (; landed < cards.Count && cards[landed].LandSeconds <= seconds; landed++)
                {
                    DrawIntoBinder(binder, cards[landed]);
                }

                using var frame = background.Clone();
                frame.Mutate(ctx => ctx.DrawImage(binder, SetValueLayout.Binder.Location, 1));

                var started = cards.Count(c => c.StartSeconds <= seconds);
                var isFinale = seconds >= finaleStart;
                DrawProgress(frame, started, cards.Count);
                DrawCounter(frame, RollingTotal(cards, seconds), isFinale);

                for (var i = landed; i < cards.Count && cards[i].FlyStartSeconds <= seconds; i++)
                {
                    DrawFlight(frame, cards[i], seconds);
                }

                if (isFinale) DrawFinale(frame, cards.Count);
                else if (started > 0) DrawFeatured(frame, cards, FeaturedIndex(cards, seconds), started, seconds);

                if (hook is not null && seconds < timing.HookSeconds + timing.WipeSeconds)
                {
                    hook.Draw(frame, HookCenterX(seconds, timing));
                }

                await sink.WriteFrame(frame);
            }
        }
        finally
        {
            cards.ForEach(c => c.Dispose());
            foreach (var image in _infoPanels.Values.Concat(_shadows.Values)) image.Dispose();
        }

        return totalFrames;
    }

    private static async Task<List<SetCard>> LoadCards(IReadOnlyList<PricedCard> pricedCards, SetValueTiming timing, double cardsStart, CardImageLoader imageLoader)
    {
        Console.WriteLine($"Loading {pricedCards.Count} card images...");
        var layout = GridLayout.Fit(pricedCards.Count, SetValueLayout.Binder, SetValueLayout.BinderGap);
        var feature = SetValueLayout.Feature;

        using var downloads = new SemaphoreSlim(ImageDownloads);
        var images = await Task.WhenAll(pricedCards.Select(async card =>
        {
            await downloads.WaitAsync();
            try
            {
                using var source = await imageLoader.Load(card.ImageUrl) ?? new Image<Rgba32>(feature.Width, feature.Height, Palette.Chip);
                var featured = source.Clone();
                ImageEffects.FitInto(featured, feature.Width, feature.Height);
                var slot = source.Clone();
                ImageEffects.FitInto(slot, layout.CellWidth, layout.CellHeight);
                return (Featured: featured, Slot: slot);
            }
            finally
            {
                downloads.Release();
            }
        }));

        var featuredSeconds = SetValuePacing.FeaturedSeconds(pricedCards.Count, timing);
        var cards = new List<SetCard>();
        var start = cardsStart;
        for (var i = 0; i < pricedCards.Count; i++)
        {
            cards.Add(new SetCard(
                pricedCards[i], pricedCards.Count - 1 - i, images[i].Featured, images[i].Slot, layout.Cell(i),
                start, featuredSeconds[i], SetValuePacing.FlySeconds(featuredSeconds[i])));
            start += featuredSeconds[i];
        }
        return cards;
    }

    /// <summary>The card in the spotlight; during the fast stretch it only changes a few times a second so it doesn't strobe.</summary>
    private static int FeaturedIndex(List<SetCard> cards, double seconds)
    {
        var latest = LastStartedAt(cards, seconds);
        if (latest < 0 || cards[latest].FeaturedSeconds >= FeaturedRefreshSeconds) return latest;

        var throttled = LastStartedAt(cards, Math.Floor(seconds / FeaturedRefreshSeconds) * FeaturedRefreshSeconds);
        return throttled < 0 ? latest : throttled;
    }

    private static int LastStartedAt(List<SetCard> cards, double seconds) => cards.FindLastIndex(c => c.StartSeconds <= seconds);

    private static decimal RollingTotal(List<SetCard> cards, double seconds)
    {
        var total = 0m;
        foreach (var card in cards)
        {
            var elapsed = seconds - card.StartSeconds;
            if (elapsed <= 0) break;
            total += card.Card.Price * (decimal)Math.Min(1, elapsed / PriceRollSeconds);
        }
        return total;
    }

    private static double HookCenterX(double seconds, SetValueTiming timing)
    {
        var progress = Math.Clamp((seconds - timing.HookSeconds) / timing.WipeSeconds, 0, 1);
        return Layout.Width / 2d - (Layout.Width + 40) * Easing.InOutCubic(progress);
    }

    private void DrawCounter(Image<Rgba32> frame, decimal total, bool isFinale)
    {
        var text = total.ToString("C2", Money);
        var font = TextDrawing.FitFont(text, _fonts.ExtraBold, SetValueLayout.CounterMaxWidth, startSize: 120, minSize: 60);
        DrawText(frame, text, font, isFinale ? Palette.Accent : Palette.Text, Layout.Width - SetValueLayout.Margin, SetValueLayout.CounterY, HorizontalAlignment.Right);
    }

    private void DrawProgress(Image<Rgba32> frame, int started, int count)
    {
        var bar = SetValueLayout.ProgressBar;
        var fill = count == 0 ? 0 : bar.Width * started / (float)count;
        if (fill > 0) frame.Mutate(ctx => ImageEffects.FillPill(ctx, Palette.Accent, bar.X, bar.Y, Math.Max(fill, bar.Height), bar.Height));

        DrawText(frame, $"{started} / {count} CARDS", _fonts.SemiBold.CreateFont(32), Palette.Muted,
            Layout.Width - SetValueLayout.Margin, SetValueLayout.CardCountY, HorizontalAlignment.Right);
    }

    private void DrawFeatured(Image<Rgba32> frame, List<SetCard> cards, int index, int started, double seconds)
    {
        var card = cards[index];
        var isSlow = card.FeaturedSeconds >= SlowCardSeconds;
        var rect = card.FeaturedRect;

        var shadow = Shadow(card.Featured.Size);
        frame.Mutate(ctx => ctx.DrawImage(shadow, new Point(rect.X - Layout.ShadowPadding, rect.Y - Layout.ShadowPadding + 24), 1));

        var scale = isSlow ? 0.85 + 0.15 * Easing.OutBack(Math.Min(1, (seconds - card.StartSeconds) / PopSeconds)) : 1;
        DrawScaled(frame, card.Featured, rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f, rect.Width * scale, rect.Height * scale);

        if (isSlow)
        {
            frame.Mutate(ctx => ctx.DrawImage(InfoPanel(card, cards.Count), new Point(SetValueLayout.InfoX, SetValueLayout.Feature.Y), 1));
            return;
        }

        var x = SetValueLayout.InfoX;
        var y = SetValueLayout.Feature.Y + 90;
        var big = _fonts.ExtraBold.CreateFont(84);
        DrawText(frame, "CARDS", big, Palette.Text, x, y, HorizontalAlignment.Left);
        DrawText(frame, $"UNDER {PriceTier(card.Card.Price)}", big, Palette.Accent, x, y + 92, HorizontalAlignment.Left);
        DrawText(frame, $"{started} / {cards.Count}", _fonts.SemiBold.CreateFont(40), Palette.Muted, x, y + 210, HorizontalAlignment.Left);
    }

    private static void DrawFlight(Image<Rgba32> frame, SetCard card, double seconds)
    {
        var progress = Easing.InOutCubic(Math.Clamp((seconds - card.FlyStartSeconds) / card.FlySeconds, 0, 1));
        var from = card.FeaturedRect;
        var to = card.SlotRect;
        float Lerp(float a, float b) => a + (b - a) * (float)progress;

        var centerX = Lerp(from.X + from.Width / 2f, to.X + to.Width / 2f);
        var centerY = Lerp(from.Y + from.Height / 2f, to.Y + to.Height / 2f);
        DrawScaled(frame, card.Featured, centerX, centerY, Lerp(from.Width, to.Width), Lerp(from.Height, to.Height));
    }

    private static void DrawScaled(Image<Rgba32> frame, Image<Rgba32> image, float centerX, float centerY, double width, double height)
    {
        var size = new Size(Math.Max(1, (int)Math.Round(width)), Math.Max(1, (int)Math.Round(height)));
        var location = new Point((int)Math.Round(centerX - size.Width / 2f), (int)Math.Round(centerY - size.Height / 2f));
        if (size == image.Size)
        {
            frame.Mutate(ctx => ctx.DrawImage(image, location, 1));
            return;
        }

        using var scaled = image.Clone(ctx => ctx.Resize(size, KnownResamplers.Triangle, compand: false));
        frame.Mutate(ctx => ctx.DrawImage(scaled, location, 1));
    }

    private void DrawFinale(Image<Rgba32> frame, int count)
    {
        TextDrawing.DrawCentered(frame, "THE FULL SET", _fonts.ExtraBold.CreateFont(120), Palette.Text, 540);
        TextDrawing.DrawCentered(frame, $"{count} CARDS", _fonts.SemiBold.CreateFont(52), Palette.Muted, 700);
    }

    private static void DrawIntoBinder(Image<Rgba32> binder, SetCard card)
    {
        var slot = card.SlotRect;
        var origin = SetValueLayout.Binder.Location;
        binder.Mutate(ctx => ctx.DrawImage(card.Slot, new Point(slot.X - origin.X, slot.Y - origin.Y), 1));
    }

    private static Image<Rgba32> RenderEmptyBinder(List<SetCard> cards)
    {
        var area = SetValueLayout.Binder;
        var binder = new Image<Rgba32>(area.Width, area.Height);
        binder.Mutate(ctx =>
        {
            foreach (var card in cards)
            {
                ctx.Fill(Palette.Chip.WithAlpha(0.55f), new RectangularPolygon(card.SlotCell.X - area.X, card.SlotCell.Y - area.Y, card.SlotCell.Width, card.SlotCell.Height));
            }
        });
        return binder;
    }

    private async Task<Image<Rgba32>> RenderBackground(SetValueSpec spec, CardImageLoader imageLoader)
    {
        var image = Backdrop.Render(glowCenterY: SetValueLayout.Feature.Y + SetValueLayout.Feature.Height / 2f);
        var logoBox = SetValueLayout.Logo;

        using var logo = spec.LogoUrl is null ? null : await imageLoader.Load(spec.LogoUrl);
        if (logo is not null)
        {
            logo.Mutate(ctx => ctx.Resize(new ResizeOptions { Size = logoBox.Size, Mode = ResizeMode.Max }));
            image.Mutate(ctx => ctx.DrawImage(logo, new Point(logoBox.X, logoBox.Y + (logoBox.Height - logo.Height) / 2), 1));
        }
        else
        {
            var titleFont = TextDrawing.FitWrappedFont(spec.Title, _fonts.ExtraBold, logoBox.Width, logoBox.Height, startSize: 64, minSize: 32);
            DrawText(image, spec.Title, titleFont, Palette.Text, logoBox.X, logoBox.Y, HorizontalAlignment.Left, logoBox.Width);
        }

        DrawText(image, "TOTAL SET VALUE", _fonts.SemiBold.CreateFont(32), Palette.Accent,
            Layout.Width - SetValueLayout.Margin, SetValueLayout.CounterLabelY, HorizontalAlignment.Right);

        var bar = SetValueLayout.ProgressBar;
        var panel = SetValueLayout.BinderPanel;
        image.Mutate(ctx =>
        {
            ImageEffects.FillPill(ctx, Palette.Chip, bar.X, bar.Y, bar.Width, bar.Height);
            ctx.Fill(Palette.BgTop.WithAlpha(0.6f), new RectangularPolygon(panel.X, panel.Y, panel.Width, panel.Height));
        });

        if (!string.IsNullOrWhiteSpace(spec.Footer))
        {
            TextDrawing.DrawCentered(image, spec.Footer, _fonts.SemiBold.CreateFont(36), Palette.Muted, SetValueLayout.FooterY);
        }
        if (!string.IsNullOrWhiteSpace(spec.PricesAsOf))
        {
            TextDrawing.DrawCentered(image, $"Market prices as of {spec.PricesAsOf}", _fonts.Medium.CreateFont(28), Palette.Muted, SetValueLayout.PricesAsOfY);
        }

        return image;
    }

    private Image<Rgba32> InfoPanel(SetCard card, int count)
    {
        if (_infoPanels.TryGetValue(card.Rank, out var cached)) return cached;

        var width = SetValueLayout.InfoWidth;
        var panel = new Image<Rgba32>(width, SetValueLayout.Feature.Height);
        float y = 50;

        var nameFont = TextDrawing.FitWrappedFont(card.Card.Name, _fonts.ExtraBold, width, 180, startSize: 68, minSize: 36);
        DrawText(panel, card.Card.Name, nameFont, Palette.Text, 0, y, HorizontalAlignment.Left, width);
        y += TextMeasurer.MeasureSize(card.Card.Name, new TextOptions(nameFont) { WrappingLength = width }).Height + 16;

        if (!string.IsNullOrWhiteSpace(card.Card.Subtitle))
        {
            var subtitleFont = TextDrawing.FitFont(card.Card.Subtitle, _fonts.SemiBold, width, startSize: 38, minSize: 26);
            DrawText(panel, card.Card.Subtitle, subtitleFont, Palette.Accent, 0, y, HorizontalAlignment.Left);
            y += TextDrawing.Height(card.Card.Subtitle, subtitleFont) + 16;
        }

        var details = string.Join(" · ", new[] { card.Card.Rarity?.ToUpperInvariant(), card.Card.Number is null ? null : $"#{card.Card.Number}" }.OfType<string>());
        if (details.Length > 0)
        {
            var detailsFont = _fonts.Medium.CreateFont(32);
            DrawText(panel, details, detailsFont, Palette.Muted, 0, y, HorizontalAlignment.Left);
            y += TextDrawing.Height(details, detailsFont) + 36;
        }

        DrawPricePill(panel, card.Card.Price.ToString("C2", Money), PillColor(card.Rank), y);

        _infoPanels[card.Rank] = panel;
        return panel;
    }

    private void DrawPricePill(Image<Rgba32> panel, string price, Color color, float y)
    {
        var font = _fonts.ExtraBold.CreateFont(68);
        var size = TextMeasurer.MeasureSize(price, new TextOptions(font));
        var height = size.Height * 1.55f;
        var width = size.Width + height;

        panel.Mutate(ctx =>
        {
            ImageEffects.FillPill(ctx, color, 0, y, width, height);
            ctx.DrawText(new RichTextOptions(font)
            {
                Origin = new Vector2(width / 2, y + height / 2),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }, price, Palette.Ink);
        });
    }

    private static Color PillColor(int rank) => rank switch
    {
        < 3 => Palette.Accent,
        < 10 => Pink,
        _ => Palette.Text
    };

    private static string PriceTier(decimal price)
    {
        var tier = PriceTiers.FirstOrDefault(t => price < t, PriceTiers[^1]);
        return tier < 1 ? tier.ToString("C2", Money) : tier.ToString("C0", Money);
    }

    private Image<Rgba32> Shadow(Size size)
    {
        if (!_shadows.TryGetValue(size, out var shadow))
        {
            using var standIn = new Image<Rgba32>(size.Width, size.Height);
            shadow = ImageEffects.BuildShadow(standIn, Layout.ShadowPadding);
            _shadows[size] = shadow;
        }
        return shadow;
    }

    private static void DrawText(Image<Rgba32> image, string text, Font font, Color color, float x, float y, HorizontalAlignment alignment, float wrappingLength = -1)
    {
        var options = new RichTextOptions(font)
        {
            Origin = new Vector2(x, y),
            HorizontalAlignment = alignment,
            VerticalAlignment = VerticalAlignment.Top,
            WrappingLength = wrappingLength,
            LineSpacing = 1.02f
        };
        image.Mutate(ctx => ctx.DrawText(options, text, color));
    }
}
