namespace CardGameDBSites.VideoGenerator.Api;

public record CardDto(int BaseId, int VariantId, string DisplayName, CardImageDto? ImageUrl, Dictionary<string, string[]>? Attributes)
{
    public string? Attribute(string name) => Attributes?.GetValueOrDefault(name)?.FirstOrDefault();
}
