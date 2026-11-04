namespace CardGameDBSites.VideoGenerator.Rendering;

public static class Easing
{
    public static double InOutCubic(double t) => t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;

    public static double OutBack(double t)
    {
        const double overshoot = 1.4;
        return 1 + (overshoot + 1) * Math.Pow(t - 1, 3) + overshoot * Math.Pow(t - 1, 2);
    }
}
