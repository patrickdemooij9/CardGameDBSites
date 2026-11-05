using CardGameDBSites.VideoGenerator.Models;

namespace CardGameDBSites.VideoGenerator.Rendering.SetValue;

/// <summary>Splits the card time so cheap cards stream in and the chase cards each get a moment.</summary>
public static class SetValuePacing
{
    private const double BaseWeight = 0.06;
    private const double Steepness = 8;

    public static double[] FeaturedSeconds(int count, SetValueTiming timing)
    {
        var weights = Enumerable.Range(0, count)
            .Select(i => BaseWeight + Math.Pow(count == 1 ? 1 : i / (double)(count - 1), Steepness))
            .ToArray();
        var totalWeight = weights.Sum();

        var seconds = weights.Select(w => timing.CardsSeconds * w / totalWeight).ToArray();
        if (count > 0) seconds[^1] += timing.LastCardExtraSeconds;
        return seconds;
    }

    public static double FlySeconds(double featuredSeconds) => Math.Clamp(featuredSeconds * 0.8, 0.3, 0.55);
}
