using CardGameDBSites.VideoGenerator.Api;
using CardGameDBSites.VideoGenerator.Models;

namespace CardGameDBSites.VideoGenerator.Specs;

/// <summary>Drafts a deck showcase spec; the groups are a starting point meant to be edited by hand.</summary>
public class DeckSpecBuilder
{
    private const int CardsPerGroup = 6;

    // First match wins, so every card lands in at most one group.
    private static readonly GroupRule[] Rules =
    [
        new("Leader & Base", 0, e => e.Type is "Leader" or "Base"),
        new("Flyers", 4, e => e.IsUnit && e.Arena == "Space"),
        new("Early game", 1, e => e.IsUnit && e.Cost <= 2),
        new("Heavy hitters", 3, e => e.IsUnit && e.Cost >= 6),
        new("Midgame", 2, e => e.IsUnit),
        new("Events & upgrades", 5, _ => true)
    ];

    public RevealVideoSpec Build(DeckDto deck, IReadOnlyList<CardDto> cards, string? hookText)
    {
        var cardsById = new Dictionary<int, CardDto>();
        foreach (var card in cards)
        {
            cardsById.TryAdd(card.BaseId, card);
            cardsById.TryAdd(card.VariantId, card);
        }

        var entries = deck.Cards
            .Where(c => cardsById.ContainsKey(c.CardId))
            .Select(c => new DeckEntry(cardsById[c.CardId], c.Amount))
            .ToList();

        var leader = entries.FirstOrDefault(e => e.Type == "Leader");
        var deckBase = entries.FirstOrDefault(e => e.Type == "Base");
        var subtitle = string.Join(" + ", new[] { leader, deckBase }.OfType<DeckEntry>().Select(e => e.ToRevealCard(false).Name));

        var groups = entries
            .GroupBy(e => Rules.First(r => r.Matches(e)))
            .OrderBy(g => g.Key.DisplayOrder)
            .Select(g => new RevealGroup(g.Key.Name, g
                .OrderByDescending(e => e.Amount).ThenBy(e => e.Cost)
                .Take(CardsPerGroup)
                .OrderBy(TypeOrder).ThenBy(e => e.Cost)
                .Select(e => e.ToRevealCard(false))
                .ToList()))
            .ToList();

        var finale = new RevealFinale("The full deck", entries
            .OrderBy(TypeOrder).ThenBy(e => e.Cost)
            .Select(e => e.ToRevealCard(true))
            .ToList());

        var hook = string.IsNullOrWhiteSpace(hookText) ? null : new RevealHook(hookText, leader?.Card.ImageUrl?.Url);

        return new RevealVideoSpec(deck.Name.ToUpperInvariant(), subtitle, "SW-UNLIMITED-DB.COM", hook, null, groups, finale, null);
    }

    private static int TypeOrder(DeckEntry entry) => entry.Type switch
    {
        "Leader" => 0,
        "Base" => 1,
        "Unit" => 2,
        _ => 3
    };
}
