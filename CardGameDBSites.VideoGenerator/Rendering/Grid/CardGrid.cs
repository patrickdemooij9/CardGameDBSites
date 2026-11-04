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
    private const float CardRatio = 0.716f;
    private const int Gap = 16;

    public static async Task<List<GridCell>> Build(IReadOnlyList<RevealCard> cards, int maxCellWidth, Fonts fonts, CardImageLoader imageLoader)
    {
        var (columns, cellWidth, cellHeight) = GridFor(cards.Count, maxCellWidth);
        var rows = (int)Math.Ceiling(cards.Count / (double)columns);
        var gridWidth = columns * cellWidth + (columns - 1) * Gap;
        var gridHeight = rows * cellHeight + (rows - 1) * Gap;
        var left = (Layout.Width - gridWidth) / 2;
        var top = Layout.GridTop + (Layout.GridBottom - Layout.GridTop - gridHeight) / 2;

        var cells = new List<GridCell>();
        for (var i = 0; i < cards.Count; i++)
        {
            var image = await imageLoader.Load(cards[i].ImageUrl) ?? new Image<Rgba32>(cellWidth, cellHeight, Palette.Chip);
            ImageEffects.FitInto(image, cellWidth, cellHeight);
            if (cards[i].Quantity > 1) DrawQuantityBadge(image, cards[i].Quantity, fonts.ExtraBold.CreateFont(cellWidth * 0.17f));

            var row = i / columns;
            var cardsInRow = Math.Min(columns, cards.Count - row * columns);
            var rowIndent = (columns - cardsInRow) * (cellWidth + Gap) / 2;
            var cellX = left + rowIndent + i % columns * (cellWidth + Gap);
            var cellY = top + row * (cellHeight + Gap);
            cells.Add(new GridCell(image, new Point(cellX + (cellWidth - image.Width) / 2, cellY + (cellHeight - image.Height) / 2)));
        }
        return cells;
    }

    private static (int Columns, int CellWidth, int CellHeight) GridFor(int count, int maxCellWidth)
    {
        var areaWidth = Layout.Width - Layout.GridSideMargin * 2;
        var areaHeight = Layout.GridBottom - Layout.GridTop;

        var best = (Columns: 1, CellWidth: 0, CellHeight: 0);
        for (var columns = 1; columns <= 8; columns++)
        {
            var rows = (int)Math.Ceiling(count / (double)columns);
            var widthLimit = (areaWidth - Gap * (columns - 1)) / (float)columns;
            var heightLimit = (areaHeight - Gap * (rows - 1)) / (float)rows * CardRatio;
            var cellWidth = (int)Math.Min(Math.Min(widthLimit, heightLimit), maxCellWidth);
            if (cellWidth > best.CellWidth) best = (columns, cellWidth, (int)(cellWidth / CardRatio));
        }
        return best;
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
