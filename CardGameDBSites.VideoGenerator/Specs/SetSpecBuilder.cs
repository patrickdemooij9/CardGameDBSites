using CardGameDBSites.VideoGenerator.Api;
using CardGameDBSites.VideoGenerator.Models;
using System.Globalization;

namespace CardGameDBSites.VideoGenerator.Specs;

public class SetSpecBuilder
{
    /// <param name="variant">Null for the normal printings.</param>
    public SetValueSpec Build(SetDto set, VariantTypeDto? variant, IReadOnlyList<CardDto> cards, string? hookText)
    {
        var pricedCards = cards
            .Where(c => c.MarketPrice is not null)
            .OrderBy(c => c.MarketPrice)
            .ThenBy(c => int.TryParse(c.Attribute("SWU Id"), out var number) ? number : int.MaxValue)
            .Select(ToPricedCard)
            .ToList();

        var title = variant is null ? set.DisplayName : $"{set.DisplayName} {variant.DisplayName}";
        var defaultHook = variant is null
            ? $"How much is a full set of {set.DisplayName} worth?"
            : $"How much are all {set.DisplayName} {variant.DisplayName} cards worth?";
        var hook = new RevealHook(string.IsNullOrWhiteSpace(hookText) ? defaultHook : hookText, set.ImageUrl);
        var pricesAsOf = DateTime.Today.ToString("d MMMM yyyy", CultureInfo.InvariantCulture);

        return new SetValueSpec(SetValueSpec.FormatName, title, set.ImageUrl, "SW-UNLIMITED-DB.COM", pricesAsOf, hook, pricedCards, null);
    }

    private static PricedCard ToPricedCard(CardDto card)
    {
        var (name, subtitle) = CardName.Split(card.DisplayName);
        return new PricedCard(name, subtitle, card.ImageUrl?.Url ?? "", card.MarketPrice!.Value, card.Attribute("Rarity"), card.Attribute("SWU Id"));
    }
}
