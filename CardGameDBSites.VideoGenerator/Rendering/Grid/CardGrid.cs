using CardGameDBSites.VideoGenerator.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;

namespace CardGameDBSites.VideoGenerator.Rendering.Grid;

/// <summary>Lays cards out as large as possible in the area between the header and the footer.</summary>
public static class CardGrid
{
    private const int Gap = 16;

    private static readonly Rectangle Area = new(
        Layout.GridSideMargin, Layout.GridTop, Layout.Width - Layout.GridSideMargin * 2, Layout.GridBottom - Layout.GridTop);

    public static async Task<List<GridCell>> Build(IReadOnlyList<RevealCard> cards, int maxCellWidth, Fonts fonts, CardImageLoader imageLoader)
    {
        var layout = GridLayout.Fit(cards.Count, Area, Gap, maxCellWidth);

        var cells = new List<GridCell>();
        for (var i = 0; i < cards.Count; i++)
        {
            var image = await imageLoader.Load(cards[i].ImageUrl) ?? new Image<Rgba32>(layout.CellWidth, layout.CellHeight, Palette.Chip);
            ImageEffects.FitInto(image, layout.CellWidth, layout.CellHeight);
            if (cards[i].Quantity > 1) DrawQuantityBadge(image, cards[i].Quantity, fonts.ExtraBold.CreateFont(layout.CellWidth * 0.17f));

            var cell = layout.Cell(i);
            cells.Add(new GridCell(image, new Point(cell.X + (cell.Width - image.Width) / 2, cell.Y + (cell.Height - image.Height) / 2)));
        }
        return cells;
    }

    private static void DrawQuantityBadge(Image<Rgba32> image, int quantity, Font font)
    {
        var text = $"x{quantity}";
        var size = TextMeasurer.MeasureSize(text, new TextOptions(font));
        var height = size.Height * 1.5f;
        var width = size.Width + height;
        var x = image.Width - width - image.Width * 0.05f;
        var y = image.Height - height - image.Width * 0.05f;

        image.Mutate(ctx =>
        {
            ImageEffects.FillPill(ctx, Palette.Accent, x, y, width, height);
            ctx.DrawText(new RichTextOptions(font)
            {
                Origin = new Vector2(x + width / 2, y + height / 2),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }, text, Palette.Ink);
        });
    }
}
