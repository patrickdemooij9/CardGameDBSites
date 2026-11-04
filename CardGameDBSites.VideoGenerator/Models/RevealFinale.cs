namespace CardGameDBSites.VideoGenerator.Models;

/// <summary>Full-deck grid at the end. Without <see cref="Cards"/> it shows every card from the groups.</summary>
public record RevealFinale(string? Title, IReadOnlyList<RevealCard>? Cards);
