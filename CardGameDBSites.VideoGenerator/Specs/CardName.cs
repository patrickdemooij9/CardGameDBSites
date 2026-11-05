namespace CardGameDBSites.VideoGenerator.Specs;

public static class CardName
{
    /// <summary>"Han Solo, Scruffy-Looking Nerf Herder" → ("Han Solo", "Scruffy-Looking Nerf Herder").</summary>
    public static (string Name, string? Subtitle) Split(string displayName)
    {
        var parts = displayName.Split(", ", 2);
        return (parts[0], parts.ElementAtOrDefault(1));
    }
}
