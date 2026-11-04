using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CardGameDBSites.VideoGenerator.Rendering.Slides;

public sealed class CardSlide(Image<Rgba32> card, Image<Rgba32> shadow, Image<Rgba32> label) : ISlide
{
    public void Draw(Image<Rgba32> frame, double centerX)
    {
        var cardLeft = (int)Math.Round(centerX - card.Width / 2d);
        var cardTop = Layout.CardCenterY - card.Height / 2;
        var labelLeft = (int)Math.Round(centerX - label.Width / 2d);

        frame.Mutate(ctx => ctx
            .DrawImage(shadow, new Point(cardLeft - Layout.ShadowPadding, cardTop - Layout.ShadowPadding + 24), 1)
            .DrawImage(card, new Point(cardLeft, cardTop), 1)
            .DrawImage(label, new Point(labelLeft, cardTop + card.Height + Layout.LabelGap), 1));
    }

    public void Dispose()
    {
        card.Dispose();
        shadow.Dispose();
        label.Dispose();
    }
}
