using CardGameDBSites.VideoGenerator.Api;
using CardGameDBSites.VideoGenerator.Models;

namespace CardGameDBSites.VideoGenerator.Specs;

public record DeckEntry(CardDto Card, int Amount)
{
    public string Type => Card.Attribute("Card Type") ?? "";
    public string Arena => Card.Attribute("Location") ?? "";
    public int Cost => int.TryParse(Card.Attribute("Cost"), out var cost) ? cost : 0;

    public bool IsUnit => Type == "Unit";

    public RevealCard ToRevealCard(bool withQuantity)
    {
        var parts = Card.DisplayName.Split(", ", 2);
        return new RevealCard(parts[0], parts.ElementAtOrDefault(1), Card.ImageUrl?.Url ?? "", withQuantity ? Amount : 1);
    }
}
