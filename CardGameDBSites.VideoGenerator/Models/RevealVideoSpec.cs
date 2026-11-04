namespace CardGameDBSites.VideoGenerator.Models;

public record RevealVideoSpec(
    string? Title,
    string? Subtitle,
    string? Footer,
    RevealHook? Hook,
    IReadOnlyList<RevealCard>? Cards,
    IReadOnlyList<RevealGroup>? Groups,
    RevealFinale? Finale,
    RevealTiming? Timing)
{
    public IReadOnlyList<RevealGroup> AllGroups() => Groups ?? [new RevealGroup(null, Cards ?? [])];

    public IReadOnlyList<RevealCard> FinaleCards() =>
        Finale?.Cards ?? AllGroups().SelectMany(g => g.Cards).ToList();
}
