using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CardGameDBSites.VideoGenerator.Rendering;

public static class ImageEffects
{
    public static void FitInto(Image<Rgba32> image, int width, int height)
    {
        image.Mutate(ctx => ctx.Resize(new ResizeOptions { Size = new Size(width, height), Mode = ResizeMode.Max }));
        ApplyRoundedCorners(image, image.Width * 0.04f);
    }

    public static Image<Rgba32> BuildShadow(Image<Rgba32> card, int padding)
    {
        var shadow = new Image<Rgba32>(card.Width + padding * 2, card.Height + padding * 2);
        shadow.Mutate(ctx => ctx
            .Fill(Color.Black.WithAlpha(0.6f), new RectangularPolygon(padding, padding, card.Width, card.Height))
            .GaussianBlur(padding * 0.45f));
        return shadow;
    }

    public static void FillPill(IImageProcessingContext ctx, Color color, float x, float y, float width, float height)
    {
        var radius = height / 2f;
        ctx.Fill(color, new RectangularPolygon(x + radius, y, width - radius * 2, height))
            .Fill(color, new EllipsePolygon(x + radius, y + radius, radius))
            .Fill(color, new EllipsePolygon(x + width - radius, y + radius, radius));
    }

    private static void ApplyRoundedCorners(Image<Rgba32> image, float radius)
    {
        IPath corner = new RectangularPolygon(-0.5f, -0.5f, radius, radius)
            .Clip(new EllipsePolygon(radius - 0.5f, radius - 0.5f, radius));
        var right = image.Width - corner.Bounds.Width + 1;
        var bottom = image.Height - corner.Bounds.Height + 1;
        var corners = new PathCollection(
            corner,
            corner.RotateDegree(90).Translate(right, 0),
            corner.RotateDegree(-90).Translate(0, bottom),
            corner.RotateDegree(180).Translate(right, bottom));

        image.Mutate(ctx => ctx
            .SetGraphicsOptions(new GraphicsOptions { Antialias = true, AlphaCompositionMode = PixelAlphaCompositionMode.DestOut })
            .Fill(Color.Red, corners));
    }
}
