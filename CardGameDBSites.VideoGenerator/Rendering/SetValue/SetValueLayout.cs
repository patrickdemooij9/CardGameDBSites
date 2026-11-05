using SixLabors.ImageSharp;

namespace CardGameDBSites.VideoGenerator.Rendering.SetValue;

public static class SetValueLayout
{
    public const int Margin = 40;
    public const int ContentWidth = Layout.Width - Margin * 2;

    public static readonly Rectangle Logo = new(Margin, 90, 400, 170);
    public const int CounterLabelY = 100;
    public const int CounterY = 140;
    public const int CounterMaxWidth = 600;

    public static readonly Rectangle ProgressBar = new(Margin, 300, ContentWidth, 18);
    public const int CardCountY = 334;

    public static readonly Rectangle Feature = new(60, 410, 360, 500);
    public const int InfoX = 470;
    public const int InfoWidth = Layout.Width - InfoX - Margin;

    public static readonly Rectangle BinderPanel = new(Margin, 960, ContentWidth, 740);
    public static readonly Rectangle Binder = new(BinderPanel.X + 16, BinderPanel.Y + 16, BinderPanel.Width - 32, BinderPanel.Height - 32);
    public const int BinderGap = 4;

    public const int FooterY = 1740;
    public const int PricesAsOfY = 1792;
}
