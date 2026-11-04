using CardGameDBSites.VideoGenerator.Api;
using CardGameDBSites.VideoGenerator.Specs;
using System.Text.Json;

namespace CardGameDBSites.VideoGenerator.Commands;

public static class DeckSpecCommand
{
    private const string DefaultApiUrl = "https://api.sw-unlimited-db.com";

    public static async Task<int> Run(string[] args)
    {
        if (!int.TryParse(args[1], out var deckId))
        {
            Console.Error.WriteLine($"'{args[1]}' is not a deck id");
            return 1;
        }
        var outputPath = args[2];
        var api = new SiteApiClient(CommandArgs.Option(args, "--api") ?? DefaultApiUrl);

        var deck = await api.GetDeck(deckId);
        var cards = await api.GetCards(deck.Cards.Select(c => c.CardId));
        var spec = new DeckSpecBuilder().Build(deck, cards, CommandArgs.Option(args, "--hook"));

        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(spec, SpecJson.Options));

        Console.WriteLine($"Wrote '{deck.Name}' to {outputPath}:");
        foreach (var group in spec.AllGroups())
        {
            Console.WriteLine($"  {group.Name}: {string.Join(", ", group.Cards.Select(c => c.Name))}");
        }
        return 0;
    }
}
