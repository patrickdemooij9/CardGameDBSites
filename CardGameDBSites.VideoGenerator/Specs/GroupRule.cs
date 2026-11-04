namespace CardGameDBSites.VideoGenerator.Specs;

public record GroupRule(string Name, int DisplayOrder, Func<DeckEntry, bool> Matches);
