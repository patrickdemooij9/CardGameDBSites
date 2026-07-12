using Microsoft.AspNetCore.Hosting;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkytearHorde.Business.Facts;
using System.Numerics;

namespace SkytearHorde.Business.Exports
{
    /// <summary>
    /// Renders "Did you know?" fact carousels (1080x1920) in the same style as the tournament ones.
    /// Slide 1 is the hook; the remaining slides render each <see cref="InfographicFactSlide"/>.
    /// </summary>
    public class FactInfographicExport : InfographicRendererBase
    {
        public FactInfographicExport(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment)
        {
        }

        /// <summary>Total slides for this fact (hook + data slides).</summary>
        public static int SlideCount(FactInfographicData data) => 1 + data.Slides.Count;

        public Task<byte[]> Render(FactInfographicData data, int slide)
        {
            if (slide <= 1) return RenderHookSlide(data);
            return RenderFactSlide(data.Slides[slide - 2], data.FooterText);
        }

        // ---- Slide 1: Hook -------------------------------------------------

        private async Task<byte[]> RenderHookSlide(FactInfographicData data)
        {
            using var image = NewCanvas();
            RenderBackground(image);
            await RenderLogo(image, 150);

            DrawCenteredText(image, "DID YOU KNOW?", SemiBold.CreateFont(40), Accent, 540, letterSpacing: 8f);

            // Hook sentence, auto-fit + wrapped, vertically centred-ish.
            var hookFont = FitFont(data.Hook, ExtraBold, Width - Margin * 2, 720, startSize: 104, minSize: 46);
            var hookOptions = new RichTextOptions(hookFont)
            {
                Origin = new Vector2(Width / 2f, 720),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                WrappingLength = Width - Margin * 2,
                LineSpacing = 1.08f
            };
            image.Mutate(ctx => ctx.DrawText(hookOptions, data.Hook, TextColor));

            RenderCtaPill(image, "Swipe to see", 1700);
            RenderFooter(image, data.FooterText);

            return await ToPng(image);
        }

        // ---- Data slides ---------------------------------------------------

        private async Task<byte[]> RenderFactSlide(InfographicFactSlide slide, string? footerText)
        {
            using var image = NewCanvas();
            RenderBackground(image);

            if (!string.IsNullOrWhiteSpace(slide.Heading))
            {
                DrawCenteredText(image, slide.Heading!, SemiBold.CreateFont(40), Accent, 140, letterSpacing: 6f);
            }

            if (slide.Kind == FactSlideKind.HeroCard)
            {
                await RenderHeroCard(image, slide);
            }
            else
            {
                RenderList(image, slide);
            }

            RenderFooter(image, footerText);
            return await ToPng(image);
        }

        private async Task RenderHeroCard(Image<Rgba32> image, InfographicFactSlide slide)
        {
            // Hero card image, whole card fitted (uncropped), centred.
            var cardBox = new Size(560, 784); // ~0.716 ratio
            var cardX = (Width - cardBox.Width) / 2;
            const int cardY = 250;
            await RenderCardImage(image, slide.ImageUrl, new Point(cardX, cardY), cardBox, crop: false);

            // Card name
            if (!string.IsNullOrWhiteSpace(slide.Title))
            {
                var nameFont = FitFont(slide.Title!, Bold, Width - Margin * 2, 150, startSize: 60, minSize: 34);
                var nameOptions = new RichTextOptions(nameFont)
                {
                    Origin = new Vector2(Width / 2f, 1080),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    WrappingLength = Width - Margin * 2
                };
                image.Mutate(ctx => ctx.DrawText(nameOptions, slide.Title!, TextColor));
            }

            // Big stat
            if (!string.IsNullOrWhiteSpace(slide.BigValue))
            {
                DrawCenteredText(image, slide.BigValue!, ExtraBold.CreateFont(150), Accent, 1250);
            }
            if (!string.IsNullOrWhiteSpace(slide.BigLabel))
            {
                DrawCenteredText(image, slide.BigLabel!, Bold.CreateFont(52), TextColor, 1450, letterSpacing: 10f);
            }

            // Optional caption (e.g. the list of traits)
            if (!string.IsNullOrWhiteSpace(slide.Caption))
            {
                var captionOptions = new RichTextOptions(Medium.CreateFont(36))
                {
                    Origin = new Vector2(Width / 2f, 1560),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    WrappingLength = Width - Margin * 2,
                    LineSpacing = 1.15f
                };
                image.Mutate(ctx => ctx.DrawText(captionOptions, slide.Caption!, MutedColor));
            }
        }

        private void RenderList(Image<Rgba32> image, InfographicFactSlide slide)
        {
            // Big count
            if (!string.IsNullOrWhiteSpace(slide.BigValue))
            {
                DrawCenteredText(image, slide.BigValue!, ExtraBold.CreateFont(160), Accent, 300);
            }
            if (!string.IsNullOrWhiteSpace(slide.BigLabel))
            {
                DrawCenteredText(image, slide.BigLabel!, Bold.CreateFont(52), TextColor, 500, letterSpacing: 8f);
            }

            var items = slide.Items ?? [];
            if (items.Count == 0) return;

            const int listTop = 640;
            const int listBottom = 1770;
            var slot = (listBottom - listTop) / items.Count;
            var fontSize = Math.Clamp(slot * 0.5f, 34f, 64f);

            for (var i = 0; i < items.Count; i++)
            {
                var y = listTop + i * slot + slot / 2f;
                var options = new RichTextOptions(Bold.CreateFont(fontSize))
                {
                    Origin = new Vector2(Width / 2f, y),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                image.Mutate(ctx => ctx.DrawText(options, items[i], TextColor));
            }
        }
    }

    public class FactInfographicData
    {
        public required string Hook { get; set; }
        public required IReadOnlyList<InfographicFactSlide> Slides { get; set; }
        public string? FooterText { get; set; }
    }

    public class InfographicFactSlide
    {
        public required FactSlideKind Kind { get; set; }
        public string? Heading { get; set; }
        public string? Title { get; set; }
        public string? BigValue { get; set; }
        public string? BigLabel { get; set; }
        public string? Caption { get; set; }
        public string? ImageUrl { get; set; }
        public IReadOnlyList<string>? Items { get; set; }
    }
}
