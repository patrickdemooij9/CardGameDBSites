using CardGameDBSites.VideoGenerator.Models;
using CardGameDBSites.VideoGenerator.Rendering.Grid;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;

namespace CardGameDBSites.VideoGenerator.Rendering.Slides;

public class SlideFactory(Fonts fonts, CardImageLoader imageLoader)
{
    public async Task<List<CarouselEntry>> Build(RevealHook? hook, IReadOnlyList<RevealGroup> groups, RevealTiming timing)
    {
        var entries = new List<CarouselEntry>();
        if (hook is not null)
        {
            entries.Add(new CarouselEntry(await BuildHook(hook), null, timing.HookSeconds));
        }
        for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
        {
            var group = groups[groupIndex];
            if (!string.IsNullOrWhiteSpace(group.Name))
            {
                var cells = await CardGrid.Build(group.Cards, Layout.CardBoxWidth, fonts, imageLoader);
                entries.Add(new CarouselEntry(new GroupGridSlide(cells), groupIndex, timing.HoldSeconds));
                continue;
            }
            foreach (var card in group.Cards)
            {
                entries.Add(new CarouselEntry(await BuildCard(card), groupIndex, timing.HoldSeconds));
            }
        }
        return entries;
    }

    private async Task<HookSlide> BuildHook(RevealHook hook)
    {
        const int textTop = 240;
        const int textWidth = Layout.Width - 120;
        var image = hook.ImageUrl is null ? null : await imageLoader.Load(hook.ImageUrl);
        var textBottom = image is null ? 1500 : 860;

        var screen = Backdrop.Render(glowCenterY: image is null ? 860 : 1300);
        var font = TextDrawing.FitWrappedFont(hook.Text, fonts.ExtraBold, textWidth, textBottom - textTop, startSize: 120, minSize: 56);
        var textOptions = new RichTextOptions(font)
        {
            Origin = new Vector2(Layout.Width / 2f, (textTop + textBottom) / 2f),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            WrappingLength = textWidth,
            LineSpacing = 1.05f
        };
        screen.Mutate(ctx => ctx.DrawText(textOptions, hook.Text, Palette.Text));

        if (image is not null)
        {
            using (image)
            {
                ImageEffects.FitInto(image, 620, 760);
                using var shadow = ImageEffects.BuildShadow(image, Layout.ShadowPadding);
                var left = (Layout.Width - image.Width) / 2;
                var top = 1300 - image.Height / 2;
                screen.Mutate(ctx => ctx
                    .DrawImage(shadow, new Point(left - Layout.ShadowPadding, top - Layout.ShadowPadding + 24), 1)
                    .DrawImage(image, new Point(left, top), 1));
            }
        }

        return new HookSlide(screen);
    }

    private async Task<ISlide> BuildCard(RevealCard card)
    {
        var image = await imageLoader.Load(card.ImageUrl) ?? new Image<Rgba32>(Layout.CardBoxWidth, Layout.CardBoxHeight, Palette.Chip);
        ImageEffects.FitInto(image, Layout.CardBoxWidth, Layout.CardBoxHeight);

        return new CardSlide(image, ImageEffects.BuildShadow(image, Layout.ShadowPadding), BuildLabel(card));
    }

    private Image<Rgba32> BuildLabel(RevealCard card)
    {
        const int width = Layout.LabelWidth;
        var label = new Image<Rgba32>(width, 220);

        var nameFont = TextDrawing.FitFont(card.Name, fonts.ExtraBold, width - 40, startSize: 72, minSize: 44);
        label.Mutate(ctx => ctx.DrawText(TextDrawing.TopCenteredOptions(nameFont, width / 2f, 0), card.Name, Palette.Text));

        if (!string.IsNullOrWhiteSpace(card.Subtitle))
        {
            var subtitleFont = TextDrawing.FitFont(card.Subtitle, fonts.SemiBold, width - 40, startSize: 44, minSize: 30);
            var subtitleY = TextDrawing.Height(card.Name, nameFont) + 20;
            label.Mutate(ctx => ctx.DrawText(TextDrawing.TopCenteredOptions(subtitleFont, width / 2f, subtitleY), card.Subtitle, Palette.Accent));
        }

        return label;
    }
}
