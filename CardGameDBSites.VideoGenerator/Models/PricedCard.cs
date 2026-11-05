namespace CardGameDBSites.VideoGenerator.Models;

public record PricedCard(string Name, string? Subtitle, string ImageUrl, decimal Price, string? Rarity, string? Number);
