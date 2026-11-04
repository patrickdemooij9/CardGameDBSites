namespace CardGameDBSites.VideoGenerator.Models;

public record RevealCard(string Name, string? Subtitle, string ImageUrl, int Quantity = 1);
