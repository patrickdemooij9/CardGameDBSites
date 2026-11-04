using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CardGameDBSites.VideoGenerator.Rendering;

public static class Backdrop
{
    public static Image<Rgba32> Render(float glowCenterY = Layout.CardCenterY)
    {
        var image = new Image<Rgba32>(Layout.Width, Layout.Height);
        image.Mutate(ctx => ctx.Fill(new LinearGradientBrush(
            new PointF(0, 0), new PointF(0, Layout.Height), GradientRepetitionMode.None,
            new ColorStop(0f, Palette.BgTop), new ColorStop(1f, Palette.BgBottom))));

        using var glow = new Image<Rgba32>(Layout.Width, Layout.Height);
        glow.Mutate(ctx => ctx
            .Fill(Palette.Accent.WithAlpha(0.20f), new EllipsePolygon(Layout.Width / 2f, glowCenterY, 560, 640))
            .GaussianBlur(140));
        image.Mutate(ctx => ctx.DrawImage(glow, 1));
        return image;
    }
}
