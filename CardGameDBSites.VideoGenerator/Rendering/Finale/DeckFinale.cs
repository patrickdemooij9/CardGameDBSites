using CardGameDBSites.VideoGenerator.Models;
using CardGameDBSites.VideoGenerator.Rendering.Grid;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CardGameDBSites.VideoGenerator.Rendering.Finale;

public sealed class DeckFinale(IReadOnlyList<GridCell> cells, RevealTiming timing) : IDisposable
{
    public double DurationSeconds => timing.FinaleStaggerSeconds + timing.FinaleFlySeconds + timing.FinaleHoldSeconds;

    public static async Task<DeckFinale> Build(IReadOnlyList<RevealCard> cards, RevealTiming timing, Fonts fonts, CardImageLoader imageLoader) =>
        new(await CardGrid.Build(cards, int.MaxValue, fonts, imageLoader), timing);

    public void Draw(Image<Rgba32> frame, double secondsSinceStart)
    {
        var delay = cells.Count > 1 ? timing.FinaleStaggerSeconds / (cells.Count - 1) : 0;
        for (var i = 0; i < cells.Count; i++)
        {
            var progress = (secondsSinceStart - i * delay) / timing.FinaleFlySeconds;
            if (progress <= 0) continue;

            var cell = cells[i];
            var travel = Layout.Width - cell.Target.X + 40;
            var x = cell.Target.X + (int)Math.Round(travel * (1 - Easing.OutBack(Math.Min(progress, 1))));
            frame.Mutate(ctx => ctx.DrawImage(cell.Image, new Point(x, cell.Target.Y), 1));
        }
    }

    public void Dispose()
    {
        foreach (var cell in cells) cell.Dispose();
    }
}
