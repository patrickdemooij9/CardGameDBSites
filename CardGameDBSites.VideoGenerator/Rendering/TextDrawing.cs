using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;

namespace CardGameDBSites.VideoGenerator.Rendering;

public static class TextDrawing
{
    public static void DrawCentered(Image<Rgba32> image, string text, Font font, Color color, float y) =>
        image.Mutate(ctx => ctx.DrawText(TopCenteredOptions(font, image.Width / 2f, y), text, color));

    public static RichTextOptions TopCenteredOptions(Font font, float x, float y) => new(font)
    {
        Origin = new Vector2(x, y),
        HorizontalAlignment = HorizontalAlignment.Center,
        TextAlignment = TextAlignment.Center,
        VerticalAlignment = VerticalAlignment.Top
    };

    public static float Height(string text, Font font) => TextMeasurer.MeasureSize(text, new TextOptions(font)).Height;

    public static Font FitWrappedFont(string text, FontFamily family, float maxWidth, float maxHeight, float startSize, float minSize)
    {
        for (var size = startSize; size > minSize; size -= 4f)
        {
            var font = family.CreateFont(size);
            var measured = TextMeasurer.MeasureSize(text, new TextOptions(font) { WrappingLength = maxWidth });
            if (measured.Width <= maxWidth && measured.Height <= maxHeight) return font;
        }
        return family.CreateFont(minSize);
    }

    public static Font FitFont(string text, FontFamily family, float maxWidth, float startSize, float minSize)
    {
        for (var size = startSize; size > minSize; size -= 4f)
        {
            var font = family.CreateFont(size);
            if (TextMeasurer.MeasureSize(text, new TextOptions(font)).Width <= maxWidth) return font;
        }
        return family.CreateFont(minSize);
    }
}
