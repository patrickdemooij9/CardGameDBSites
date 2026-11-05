namespace CardGameDBSites.VideoGenerator.Api;

public record CardDto(
    int BaseId,
    int VariantId,
    string DisplayName,
    CardImageDto? ImageUrl,
    Dictionary<string, string[]>? Attributes,
    IReadOnlyList<CardVariantDto>? Variants)
{
    public string? Attribute(string name) => Attributes?.GetValueOrDefault(name)?.FirstOrDefault();

    public decimal? MarketPrice => Variants?.FirstOrDefault(v => v.CardVariantId == VariantId)?.MarketPrice;
}
