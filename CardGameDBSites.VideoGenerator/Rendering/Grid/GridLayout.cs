using SixLabors.ImageSharp;

namespace CardGameDBSites.VideoGenerator.Rendering.Grid;

/// <summary>The largest card cells that fit <see cref="Count"/> cards in an area, centred, with a centred last row.</summary>
public sealed record GridLayout(int Count, int Columns, int CellWidth, int CellHeight, int Gap, Rectangle Area)
{
    private const float CardRatio = 0.716f;

    public static GridLayout Fit(int count, Rectangle area, int gap, int maxCellWidth = int.MaxValue)
    {
        var best = new GridLayout(count, 1, 0, 0, gap, area);
        for (var columns = 1; columns <= 40; columns++)
        {
            var rows = (int)Math.Ceiling(count / (double)columns);
            var widthLimit = (area.Width - gap * (columns - 1)) / (float)columns;
            var heightLimit = (area.Height - gap * (rows - 1)) / (float)rows * CardRatio;
            var cellWidth = (int)Math.Min(Math.Min(widthLimit, heightLimit), maxCellWidth);
            if (cellWidth > best.CellWidth) best = best with { Columns = columns, CellWidth = cellWidth, CellHeight = (int)(cellWidth / CardRatio) };
        }
        return best;
    }

    public Rectangle Cell(int index)
    {
        var rows = (int)Math.Ceiling(Count / (double)Columns);
        var gridWidth = Columns * CellWidth + (Columns - 1) * Gap;
        var gridHeight = rows * CellHeight + (rows - 1) * Gap;

        var row = index / Columns;
        var cardsInRow = Math.Min(Columns, Count - row * Columns);
        var rowIndent = (Columns - cardsInRow) * (CellWidth + Gap) / 2;

        var x = Area.X + (Area.Width - gridWidth) / 2 + rowIndent + index % Columns * (CellWidth + Gap);
        var y = Area.Y + (Area.Height - gridHeight) / 2 + row * (CellHeight + Gap);
        return new Rectangle(x, y, CellWidth, CellHeight);
    }
}
