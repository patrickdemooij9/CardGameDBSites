namespace CardGameDBSites.VideoGenerator.Models;

public record RevealGroup(string? Name, IReadOnlyList<RevealCard> Cards);
