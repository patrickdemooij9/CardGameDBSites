namespace CardGameDBSites.VideoGenerator.Models;

/// <summary>A "how much is the full set worth?" video: cards land in a binder from cheapest to most expensive.</summary>
public record SetValueSpec(
    string Format,
    string Title,
    string? LogoUrl,
    string? Footer,
    string? PricesAsOf,
    RevealHook? Hook,
    IReadOnlyList<PricedCard> Cards,
    SetValueTiming? Timing)
{
    public const string FormatName = "set-value";
}
