namespace CardGameDBSites.VideoGenerator.Api;

public record CardPageDto(IReadOnlyList<CardDto> Items, int TotalItems);
