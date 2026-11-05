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
        var (name, subtitle) = CardName.Split(Card.DisplayName);
        return new RevealCard(name, subtitle, Card.ImageUrl?.Url ?? "", withQuantity ? Amount : 1);
    }
}
