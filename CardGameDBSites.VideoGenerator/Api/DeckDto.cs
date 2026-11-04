namespace CardGameDBSites.VideoGenerator.Api;

public record DeckDto(int Id, string Name, IReadOnlyList<DeckCardDto> Cards);
