namespace CardGameDBSites.VideoGenerator.Commands;

public static class CommandArgs
{
    public const string DefaultApiUrl = "https://api.sw-unlimited-db.com";

    public static string? Option(string[] args, string name)
    {
        var index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}
